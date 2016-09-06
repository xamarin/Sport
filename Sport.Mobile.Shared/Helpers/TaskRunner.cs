using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Sport.Mobile.Shared
{
	public static class TaskRunner
	{
		//public static event EventHandler<Task> BeforeTaskRun;
		//public static event EventHandler<Task> AfterTaskRun;
		//public static Func<Task, bool> ShouldRunTask;

		public static async Task RunSafe(Task task, Action<Exception> onError = null, CancellationToken token = default(CancellationToken))
		{
			Exception exception = null;

			try
			{
				if(!token.IsCancellationRequested)
				{
					await Task.Run(() => {
						task.Start();
						task.Wait();
					});
				}
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
			finally
			{
				//AfterTaskRun?.Invoke(null, task);
			}

			if(exception != null)
			{
				//TODO Log to Insights
				Debug.WriteLine(exception);
				onError?.Invoke(exception);
			}
		}
	}
}

