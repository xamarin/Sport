using Foundation;
using ImageCircle.Forms.Plugin.iOS;
using Microsoft.WindowsAzure.MobileServices;
using UIKit;
using System;
using Xamarin.Forms;
using System.Diagnostics;
using Newtonsoft.Json;
using Sport.Mobile.Shared;
using Microsoft.Azure.Mobile;

namespace Sport.Mobile.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
		{
			//#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
            //#endif

            CurrentPlatform.Init();
			SQLitePCL.CurrentPlatform.Init();
			Forms.Init();
			ImageCircleRenderer.Init();
			XFGloss.iOS.Library.Init();

			MobileCenter.Configure(Keys.MobileCenterKeyiOS);
			LoadApplication(new App());

			UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
			UIBarButtonItem.Appearance.SetBackButtonTitlePositionAdjustment(new UIOffset(0, -100), UIBarMetrics.Default);

			AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
				try
				{
					var exception = ((Exception)e.ExceptionObject).GetBaseException();
					Console.WriteLine("**SPORT UNHANDLED EXCEPTION**\n\n" + exception);
					exception.Track();
				}
				catch
				{
					throw;
				}
			};

			return base.FinishedLaunching(uiApplication, launchOptions);
		}

		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			App.Instance.CurrentAthlete.DeviceToken = deviceToken.Description.Trim('<', '>').Replace(" ", "");
			MessagingCenter.Send(App.Instance, Shared.Messages.RegisteredForRemoteNotifications);
		}

		public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
		{
			Debug.WriteLine("FailedToRegisterForRemoteNotifications called");
			Debug.WriteLine(error);
			MessagingCenter.Send(App.Instance, Shared.Messages.RegisteredForRemoteNotifications);
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
					MessagingCenter.Send(App.Instance, Shared.Messages.IncomingPayloadReceivedInternal, payloadValue);
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