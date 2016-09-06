using System;
using System.Threading.Tasks;
using MessageBar;
using Sport.Mobile.Shared;
using Xamarin.Forms;

[assembly: Dependency(typeof(Sport.Mobile.iOS.ToastNotifier))]

namespace Sport.Mobile.iOS
{
	public class ToastNotifier : IToastNotifier
	{
		public static void Init()
		{
			//_styleSheet = new MessageBarStyleSheet();
		}

		public Task<bool> Notify(ToastNotificationType type, string title, string description, TimeSpan duration, object context = null)
		{
			MessageType msgType = MessageType.Info;

			switch(type)
			{
				case ToastNotificationType.Error:
				case ToastNotificationType.Warning:
					msgType = MessageType.Error;
					break;

				case ToastNotificationType.Success:
					msgType = MessageType.Success;
					break;
			}

			var taskCompletionSource = new TaskCompletionSource<bool>();
			MessageBarManager.SharedInstance.ShowMessage(title, description, msgType, b => taskCompletionSource.TrySetResult(b));
			return taskCompletionSource.Task;
		}

		public void HideAll()
		{
			MessageBarManager.SharedInstance.HideAll();
		}
	}
}