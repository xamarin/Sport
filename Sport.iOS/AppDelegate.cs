using System.Diagnostics;
using Foundation;
using ImageCircle.Forms.Plugin.iOS;
using Newtonsoft.Json;
using Sport.Shared;
using UIKit;
using Xamarin;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Sport.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : FormsApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{

			if(InsightsManager.IsEnabled)
				Insights.Initialize(Keys.InsightsApiKey);

			Calabash.Start();

			Forms.Init();
			ImageCircleRenderer.Init();
			ToastNotifier.Init();
		
			//We're using the value of the StyleId as the accessibility identifier for use w/ Xamarin UITest / XTC
			Forms.ViewInitialized += (sender, e) =>
			{
				if(null != e.View.StyleId)
				{
					e.NativeView.AccessibilityIdentifier = e.View.StyleId;
				}
			};

			var atts = new UITextAttributes {
				Font = UIFont.FromName("SegoeUI", 22),
			};
			UINavigationBar.Appearance.SetTitleTextAttributes(atts);

			var barButtonAtts = new UITextAttributes {
				Font = UIFont.FromName("SegoeUI", 16),
			};

			UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
			UIBarButtonItem.Appearance.SetTitleTextAttributes(barButtonAtts, UIControlState.Normal);
			UIBarButtonItem.Appearance.SetBackButtonTitlePositionAdjustment(new UIOffset(0, -100), UIBarMetrics.Default);

			LoadApplication(new App());
			ProcessPendingPayload(options);

			return base.FinishedLaunching(app, options);
		}

		void ProcessPendingPayload(NSDictionary userInfo)
		{
			if(userInfo == null)
				return;

			NSObject aps, payload, innerPayload;
			if(userInfo.TryGetValue(new NSString("UIApplicationLaunchOptionsRemoteNotificationKey"), out payload))
			{
				if(((NSDictionary)payload).TryGetValue(new NSString("aps"), out aps))
				{
					if(((NSDictionary)aps).TryGetValue(new NSString("payload"), out innerPayload))
					{
						var notificationPayload = JsonConvert.DeserializeObject<NotificationPayload>(innerPayload.ToString());
						App.Current.OnIncomingPayload(notificationPayload);
					}
				}
			}
		}

		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			App.CurrentAthlete.DeviceToken = deviceToken.Description.Trim('<', '>').Replace(" ", "");
			MessagingCenter.Send<App>(App.Current, Messages.RegisteredForRemoteNotifications);
		}

		public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
		{
			Debug.WriteLine("FailedToRegisterForRemoteNotifications called");
			Debug.WriteLine(error);
			MessagingCenter.Send<App>(App.Current, Messages.RegisteredForRemoteNotifications);
		}

		public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
		{
			NSObject aps, alert, payload;

			if(!userInfo.TryGetValue(new NSString("aps"), out aps))
				return;
			
			var apsHash = aps as NSDictionary;

			NotificationPayload payloadValue = null;
			if(apsHash.TryGetValue(new NSString("payload"), out payload))
			{
				payloadValue = JsonConvert.DeserializeObject<NotificationPayload>(payload.ToString());
				if(payloadValue != null)
				{
					MessagingCenter.Send<App, NotificationPayload>(App.Current, Messages.IncomingPayloadReceivedInternal, payloadValue);
				}
			}

			var badgeValue = apsHash.ObjectForKey(new NSString("badge"));

			if(badgeValue != null)
			{
				int count;
				if(int.TryParse(new NSString(badgeValue.ToString()), out count))
				{
					//UIApplication.SharedApplication.ApplicationIconBadgeNumber = count;
				}
			}

			if(apsHash.TryGetValue(new NSString("alert"), out alert))
			{
				alert.ToString().ToToast(ToastNotificationType.Info, "Incoming notification");
			}
		}
	}
}