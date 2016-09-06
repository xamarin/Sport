using Foundation;
using Sport.Mobile.Shared;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(Sport.Mobile.iOS.PushNotifications))]

namespace Sport.Mobile.iOS
{
	public class PushNotifications : IPushNotifications
	{
		public void RegisterForPushNotifications()
		{
			var settings = UIUserNotificationSettings.GetSettingsForTypes(UIUserNotificationType.Alert
			               | UIUserNotificationType.Badge
			               | UIUserNotificationType.Sound, new NSSet());


			if (settings != null)
			{
				UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
				UIApplication.SharedApplication.RegisterForRemoteNotifications();
			}
		}

		public bool IsRegistered
		{
			get
			{
				return UIApplication.SharedApplication.IsRegisteredForRemoteNotifications;
			}
		}
	}
}

