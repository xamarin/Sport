using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Sport.Mobile.Shared
{
	public class BaseViewModel : BaseNotify, IDirty
	{
		[JsonIgnore]
		public bool IsDirty
		{
			get;
			set;
		}

		CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		bool _isBusy;

		public event EventHandler IsBusyChanged;

		public bool IsBusy
		{
			get
			{
				return _isBusy;
			}
			set
			{
				if(SetPropertyChanged(ref _isBusy, value))
				{
					if(IsBusyChanged != null)
						IsBusyChanged(this, new EventArgs());
				}
			}
		}

		public bool WasCancelledAndReset
		{
			get
			{
				var cancelled = _cancellationTokenSource != null && _cancellationTokenSource.IsCancellationRequested;

				if(cancelled)
					ResetCancellationToken();

				return cancelled;
			}
		}

		public CancellationToken CancellationToken
		{
			get
			{
				if(_cancellationTokenSource == null)
					_cancellationTokenSource = new CancellationTokenSource();

				return _cancellationTokenSource.Token;
			}
		}

		public void ResetCancellationToken()
		{
			_cancellationTokenSource = new CancellationTokenSource();
		}

		public virtual void CancelTasks()
		{
			if(!_cancellationTokenSource.IsCancellationRequested && CancellationToken.CanBeCanceled)
			{
				_cancellationTokenSource.Cancel();
			}
		}

		public virtual void NotifyPropertiesChanged([CallerMemberName] string caller = "")
		{
			Debug.WriteLine($"NotifyPropertiesChanged called for {GetType().Name} by {caller}");
		}

		public async Task RunSafe(Task task, bool notifyOnError = true, [CallerMemberName] string caller = null, [CallerLineNumber] long line = 0, [CallerFilePath] string path = null)
		{
			Exception exception = null;

			try
			{
				//if(!App.Instance.IsNetworkReachable)
				//{
				//	MessagingCenter.Send<BaseViewModel, Exception>(this, Messages.ExceptionOccurred, new WebException("Please connect to the Information Super Highway"));
				//	return;
				//}

				await TaskRunner.RunSafe(task, null, CancellationToken);
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
				//InsightsManager.Report(exception);
				Debug.WriteLine(exception);

				if(notifyOnError)
				{
					NotifyException(exception);
				}
			}
		}

		public async Task<T> RunSafe<T>(Task<T> task, bool notifyOnError = true, [CallerMemberName] string caller = null, [CallerLineNumber] long line = 0, [CallerFilePath] string path = null)
		{
			await RunSafe((Task)task, notifyOnError, caller, line, path);
			return task.Result;
		}

		public void NotifyException(Exception exception)
		{
			MessagingCenter.Send(new object(), Messages.ExceptionOccurred, exception);
		}

		public class Busy : IDisposable
		{
			readonly object _sync = new object();
			readonly BaseViewModel _viewModel;

			public Busy(BaseViewModel viewModel)
			{
				_viewModel = viewModel;
				lock(_sync)
				{
					_viewModel.IsBusy = true;
				}
			}

			public void Dispose()
			{
				lock(_sync)
				{
					_viewModel.IsBusy = false;
				}
			}
		}

		RunSafeException HandleTaskException(Exception e, Task task, string caller, long line, string path)
		{
			var desc = $"Filepath: {path}\nLine: {line}\nCalling Method: {caller}\nTask:{task}";
			var excep = new RunSafeException($"RunSafe Task error occurred from:\n{desc}\nTask: {task}", e)
			{
				CallingMethod = desc,
				Task = task,
			};

			return excep;
		}
	}

	public class RunSafeException : Exception
	{
		public RunSafeException(string message, Exception inner) : base(message, inner)
		{
		}

		public Task Task
		{
			get; set;
		}

		public string CallingMethod
		{
			get; set;
		}
	}
}