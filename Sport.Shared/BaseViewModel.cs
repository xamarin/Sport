using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Xamarin.Forms;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Sport.Shared
{
	public class BaseViewModel : BaseNotify
	{
		#region Properties

		bool _isBusy = false;
		CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public bool IsBusy
		{
			get
			{
				return _isBusy;
			}
			set
			{
				SetPropertyChanged(ref _isBusy, value);
				SetPropertyChanged("IsNotBusy");
			}
		}

		public bool IsNotBusy
		{
			get
			{
				return !IsBusy;
			}
		}

		#endregion

		public virtual void NotifyPropertiesChanged()
		{
		}

		#region Task Safety

		public Action<Exception> OnTaskException
		{
			get;
			set;
		}

		/// <summary>
		/// All tasks are created as unstarted tasks and are processe via a proxy method that will run the task safely
		/// Instead of wrapping every task body in a try/catch, we'll process tasks in RunSafe
		/// RunSafe will start the task within the scope of a try/catch block and notify the app of any exceptions
		/// This can also be used to cancel running tasks when a user navigates away from a page - each VM has a cancellation token
		/// </summary>
		public async Task RunSafe(Task task, bool notifyOnError = true, [CallerMemberName] string caller = "")
		{
			if(!App.IsNetworkRechable)
			{
				MessagingCenter.Send<BaseViewModel, Exception>(this, Messages.ExceptionOccurred, new WebException("Please connect to the Information Super Highway"));
				return;
			}

			Exception exception = null;

			try
			{
				await Task.Run(() =>
				{
					if(!CancellationToken.IsCancellationRequested)
					{
						task.Start();
						task.Wait();
					}
				});
			}
			catch(TaskCanceledException)
			{
				Debug.WriteLine("Task Cancelled");
			}
			catch(AggregateException e)
			{
				var ex = e.InnerException;
				while(ex.InnerException != null)
					ex = ex.InnerException;

				exception = ex;
			}
			catch(Exception e)
			{
				exception = e;
			}

			if(exception != null)
			{
				InsightsManager.Report(exception);
				Debug.WriteLine(exception);

				if(notifyOnError)
				{
					NotifyException(exception);
				}
			}
		}

		public void NotifyException(Exception exception)
		{
			MessagingCenter.Send<BaseViewModel, Exception>(this, Messages.ExceptionOccurred, exception);
		}

		public CancellationToken CancellationToken
		{
			get
			{
				return _cancellationTokenSource.Token;
			}
		}

		public virtual void CancelTasks()
		{
			if(!_cancellationTokenSource.IsCancellationRequested && CancellationToken.CanBeCanceled)
			{
				_cancellationTokenSource.Cancel();
				_cancellationTokenSource = new CancellationTokenSource();
			}
		}

		#endregion
	}

	#region Helper Classes

	/// <summary>
	/// Helper class that enforces the flag will always get set to false
	/// </summary>
	public class Busy : IDisposable
	{
		readonly object _sync = new object();
		readonly BaseViewModel _viewModel;

		public Busy(BaseViewModel viewModel)
		{
			_viewModel = viewModel;
			//Device.BeginInvokeOnMainThread(() =>
			//{
				lock(_sync)
				{
					_viewModel.IsBusy = true;
				}
			//});
		}

		public void Dispose()
		{
			//Device.BeginInvokeOnMainThread(() =>
			//{
				lock(_sync)
				{
					_viewModel.IsBusy = false;
				}
			//});
		}
	}

	#endregion
}