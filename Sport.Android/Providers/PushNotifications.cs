using System;
using Android.App;
using Android.Content;
using Gcm.Client;
using Newtonsoft.Json;
using Sport.Shared;
using Xamarin;
using Xamarin.Forms;
using System.Diagnostics;

[assembly: Permission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]

[assembly: UsesPermission(Name = "android.permission.GET_ACCOUNTS")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]

[assembly: Dependency(typeof(Sport.Android.PushNotifications))]

namespace Sport.Android
{
	public class PushNotifications : IPushNotifications
	{
		public void RegisterForPushNotifications()
		{
			try
			{
				GcmClient.CheckDevice(Forms.Context);
				GcmClient.CheckManifest(Forms.Context);
				GcmClient.Register(Forms.Context, GcmBroadcastReceiver.SENDER_IDS);
			}
			catch(Exception e)
			{
				InsightsManager.Report(e);
				Debug.WriteLine(e);
			}
		}
	}

	[BroadcastReceiver(Permission = Gcm.Client.Constants.PERMISSION_GCM_INTENTS)]
	[IntentFilter(new string[] {
		Gcm.Client.Constants.INTENT_FROM_GCM_MESSAGE
	}, Categories = new string[] {
		"@PACKAGE_NAME@"
	})]
	[IntentFilter(new string[] {
		Gcm.Client.Constants.INTENT_FROM_GCM_REGISTRATION_CALLBACK
	}, Categories = new string[] {
		"@PACKAGE_NAME@"
	})]
	[IntentFilter(new string[] {
		Gcm.Client.Constants.INTENT_FROM_GCM_LIBRARY_RETRY
	}, Categories = new string[] {
		"@PACKAGE_NAME@"
	})]

	public class GcmBroadcastReceiver : GcmBroadcastReceiverBase<PushHandlerService>
	{
		public static string[] SENDER_IDS = {
			Keys.GooglePushNotificationSenderId
		};

		public override void OnReceive(Context context, Intent intent)
		{
			global::Android.OS.PowerManager.WakeLock sWakeLock;
			var pm = global::Android.OS.PowerManager.FromContext(context);
			sWakeLock = pm.NewWakeLock(global::Android.OS.WakeLockFlags.Partial, "GCM Broadcast Reciever Tag");
			sWakeLock.Acquire();

			if(!HandlePushNotification(context, intent))
			{
				base.OnReceive(context, intent);
			}
		
			sWakeLock.Release();
		}

		internal static bool HandlePushNotification(Context context, Intent intent)
		{
			string message;
			if(!intent.Extras.ContainsKey("message"))
				return false;

			message = intent.Extras.Get("message").ToString();
			var title = intent.Extras.Get("title").ToString();

			var activityIntent = new Intent(context, typeof(MainActivity));
			var payloadValue = GetPayload(intent);
			activityIntent.PutExtra("payload", payloadValue);
			activityIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);
			var pendingIntent = PendingIntent.GetActivity(context, 0, activityIntent, PendingIntentFlags.UpdateCurrent);

			var n = new Notification.Builder(context);
			n.SetSmallIcon(Resource.Drawable.ic_successstatus);
			n.SetLights(global::Android.Graphics.Color.Blue, 300, 1000);
			n.SetContentIntent(pendingIntent);
			n.SetContentTitle(title);
			n.SetTicker(message);
			n.SetLargeIcon(global::Android.Graphics.BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.icon));
			n.SetSmallIcon(Resource.Drawable.ic_trophy_white);
			n.SetContentText(message);
			n.SetVibrate(new long[] {
				200,
				200,
				100,
			});

			var nm = NotificationManager.FromContext(context);
			nm.Notify(0, n.Build());

			if(MainActivity.IsRunning)
			{
				try
				{
					message.ToToast();
					var payload = GetPayload(intent);
					var pl = JsonConvert.DeserializeObject<NotificationPayload>(payload);

					if(payloadValue != null)
					{
						Device.BeginInvokeOnMainThread(() =>
						{
							MessagingCenter.Send<App, NotificationPayload>(App.Current, Messages.IncomingPayloadReceivedInternal, pl);
						});
					}
				}
				catch(Exception e)
				{
					InsightsManager.Report(e, Insights.Severity.Error);
				}
			}

			return true;
		}

		internal static string GetPayload(Intent intent)
		{
			if(intent.Extras.ContainsKey("payload"))
				return intent.Extras.Get("payload").ToString();

			return null;
		}
	}

	[Service]
	public class PushHandlerService : GcmServiceBase
	{
		public PushHandlerService() : base(GcmBroadcastReceiver.SENDER_IDS)
		{
		}

		protected override void OnRegistered(Context context, string registrationId)
		{
			try
			{
				App.CurrentAthlete.DeviceToken = registrationId;
				MessagingCenter.Send<App>(App.Current, Messages.RegisteredForRemoteNotifications);
			}
			catch(Exception e)
			{
				InsightsManager.Report(e);
				Debug.WriteLine(e);
			}
		}

		protected override void OnMessage(Context context, Intent intent)
		{
			GcmBroadcastReceiver.HandlePushNotification(context, intent);
		}

		protected override void OnUnRegistered(Context context, string registrationId)
		{
			//Receive notice that the app no longer wants notifications
		}

		protected override void OnError(Context context, string errorId)
		{
			//Some more serious error happened
			Debug.WriteLine("PushHandlerService error: " + errorId);
		}
	}
}
