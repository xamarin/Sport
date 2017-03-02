using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using ImageCircle.Forms.Plugin.Droid;
using Microsoft.Azure.Mobile;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Sport.Mobile.Shared;
using Xamarin.Forms.Platform.Android;
using Android;
using Android.Support.V4.Content;
using NControl.Controls.Droid;

namespace Sport.Mobile.Droid
{
	[Activity(Label = "Sport", Icon = "@drawable/icon", Theme = "@style/DefaultTheme", MainLauncher = false,
			  ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : FormsAppCompatActivity
	{
		public static bool IsRunning
		{
			get;
			private set;
		}

		protected override void OnCreate(Bundle bundle)
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
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
				AdjustStatusBar(0);

				base.OnCreate(bundle);
				ToolbarResource = Resource.Layout.Toolbar;

				Window.SetSoftInputMode(SoftInput.AdjustResize);
				Window.DecorView.SystemUiVisibility = StatusBarVisibility.Visible;

				CurrentPlatform.Init();
				Xamarin.Forms.Forms.Init(this, bundle);
				NControls.Init();
				ImageCircleRenderer.Init();

				MobileCenter.Configure(Keys.MobileCenterKeyAndroid);
				LoadApplication(new App());
				XFGloss.Droid.Library.Init(this, bundle);
			}
			catch(Exception e)
			{
				Console.WriteLine("**SPORT LAUNCH EXCEPTION**\n\n" + e);
				e.Track();
			}
		}

		public void AdjustStatusBar(int size)
		{
			//Temp hack until the FormsAppCompatActivity works for full screen
			if(Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
			{
				var statusBarHeightInfo = typeof(FormsAppCompatActivity).GetField("_statusBarHeight",
											  System.Reflection.BindingFlags.Instance |
											  System.Reflection.BindingFlags.NonPublic);

				statusBarHeightInfo.SetValue(this, size);
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