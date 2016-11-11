using System;
using System.Diagnostics;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using ImageCircle.Forms.Plugin.Droid;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Sport.Mobile.Shared;
using HockeyApp.Android;

namespace Sport.Mobile.Droid
{
	[Activity(Label = "Sport", Icon = "@drawable/icon", Theme = "@style/GrayTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
	{
		public static bool IsRunning
		{
			get;
			private set;
		}

		protected override void OnCreate(global::Android.OS.Bundle bundle)
		{
			//Uncomment when a HockeyApp iOS App ID is provided in Keys.cs
			//CrashManager.Register(this, Keys.HockeyAppId_Android);

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
				try
				{
					var exception = ((Exception)e.ExceptionObject).GetBaseException();
					Console.WriteLine("**SPORT UNHANDLED EXCEPTION**\n\n" + exception);
					//InsightsManager.Report(ex, Xamarin.Insights.Severity.Critical);
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

				LoadApplication(new App());
				XFGloss.Droid.Library.Init(this, bundle);

				var color = new ColorDrawable(Color.Transparent);
				ActionBar.SetIcon(color);

				Window.AddFlags(WindowManagerFlags.Fullscreen);
			}
			catch(Exception e)
			{
				Debug.WriteLine(e);
			}
		}

		protected override void OnPause()
		{
			Debug.WriteLine("PAUSE");
			IsRunning = false;
			base.OnPause();
		}

		protected override void OnResume()
		{
			Debug.WriteLine("RESUME");
			IsRunning = true;

			ProcessNotificationPayload();
			base.OnResume();
		}

		protected override void OnStop()
		{
			Debug.WriteLine("STOP");
			base.OnStop();
		}

		protected override void OnStart()
		{
			Debug.WriteLine("START");
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