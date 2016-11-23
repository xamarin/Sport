using System;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using ImageCircle.Forms.Plugin.Droid;
using Microsoft.Azure.Mobile;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Sport.Mobile.Shared;

namespace Sport.Mobile.Droid
{
	[Activity(Label = "Sport", Icon = "@drawable/icon", Theme = "@style/DefaultTheme", MainLauncher = false,
	          ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
	{
		public static bool IsRunning
		{
			get;
			private set;
		}

		protected override void OnCreate(Bundle bundle)
		{
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

			try
			{
				base.OnCreate(bundle);

				Window.SetSoftInputMode(SoftInput.AdjustPan);

				CurrentPlatform.Init();
				Xamarin.Forms.Forms.Init(this, bundle);
				ImageCircleRenderer.Init();

				MobileCenter.Configure(Keys.MobileCenterKeyAndroid);
				LoadApplication(new App());
				XFGloss.Droid.Library.Init(this, bundle);

				var color = new ColorDrawable(Color.Transparent);
				ActionBar.SetIcon(color);

				Window.AddFlags(WindowManagerFlags.Fullscreen);
			}
			catch(Exception e)
			{
				Console.WriteLine("**SPORT LAUNCH EXCEPTION**\n\n" + e);
				e.Track();
			}
		}

		protected override void OnPause()
		{
			IsRunning = false;
			base.OnPause();
		}

		protected override void OnResume()
		{
			IsRunning = true;

			ProcessNotificationPayload();
			base.OnResume();
		}

		protected override void OnStop()
		{
			base.OnStop();
		}

		protected override void OnStart()
		{
			base.OnStart();
		}

		void ProcessNotificationPayload()
		{
			if(Intent != null && Intent.Extras != null)
			{
				if(Intent.Extras.ContainsKey("payload"))
				{
					var payload = Intent.Extras.GetString("payload");
					var np = JsonConvert.DeserializeObject<NotificationPayload>(payload);
					App.Instance.OnIncomingPayload(np);
				}
			}
		}
	}
}