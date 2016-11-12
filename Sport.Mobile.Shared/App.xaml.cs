using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Sport.Mobile.Shared
{
	public partial class App : Application
	{
		#region Fields & Properties

		public static uint AnimationSpeed = 250;
		public static int DelaySpeed = 300;
		public IHUDProvider _hud;
		static App _instance;

		public static App Instance
		{
			get
			{
				return _instance;
			}
		}

		public IHUDProvider Hud
		{
			get
			{
				return _hud ?? (_hud = DependencyService.Get<IHUDProvider>());
			}
		}

		public static NotificationPayload PendingNotificationPayload
		{
			get;
			private set;
		}

		public IAuthenticator Authenticator
		{
			get;
			private set;
		}

		public Theming Theming
		{
			get;
			set;
		} = new Theming();

		public bool IsNetworkReachable
		{
			get;
			set;
		}

		public Athlete CurrentAthlete
		{
			get;
			set;
		}

		public List<string> PraisePhrases
		{
			get;
			set;
		} = new List<string>();

		#endregion

		public App()
		{
			_instance = this;

			SetDefaultPropertyValues();
			InitializeComponent();
			MessagingCenter.Subscribe<object, Exception>(this, Messages.ExceptionOccurred, OnAppExceptionOccurred);

			if(Settings.AthleteId == null || !Settings.RegistrationComplete)
			{
				StartRegistrationFlow();
			}
			else
			{
				StartAuthenticationFlow();
			}
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}

		/// <summary>
		/// This method is here purely to handle shelved push notifications
		/// </summary>
		/// <param name="payload">Payload.</param>
		public void OnIncomingPayload(NotificationPayload payload)
		{
			if(payload == null)
				return;

			if(App.Instance.CurrentAthlete == null)
			{
				PendingNotificationPayload = payload;
			}
			else
			{
				MessagingCenter.Send(Instance, Messages.IncomingPayloadReceived, payload);
			}
		}

		internal void ProcessPendingPayload()
		{
			if(PendingNotificationPayload == null)
				return;

			MessagingCenter.Send(Instance, Messages.IncomingPayloadReceived, PendingNotificationPayload);
			PendingNotificationPayload = null;
		}

		/// <summary>
		/// Kicks off the main application flow - this is the typical route taken once a user is registered
		/// </summary>
		void StartAuthenticationFlow()
		{
			//Create our entry page and add it to a NavigationPage, then apply a randomly assigned color theme
			var page = new AthleteLeaguesPage(Instance.CurrentAthlete);
			var navPage = new ThemedNavigationPage(page);
			page.ApplyTheme(navPage);
			MainPage = navPage;

			page.EnsureUserAuthenticated();
		}

		/// <summary>
		/// Kicks off the registration flow so the user can register and authenticate
		/// </summary>
		internal void StartRegistrationFlow()
		{
			MainPage = new WelcomeStartPage().WithinNavigationPage();
		}

		void OnAppExceptionOccurred(object sender, Exception exception)
		{
			Device.BeginInvokeOnMainThread(async () => {
				try
				{
					if(_hud != null)
					{
						_hud.Dismiss();
					}

					var msg = exception.Message;
					var mse = exception as MobileServiceInvalidOperationException;

					if(mse != null)
					{
						var body = await mse.Response.Content.ReadAsStringAsync();
						var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(body);

						if(dict != null && dict.ContainsKey("message"))
							msg = dict["message"].ToString();
					}

					//if(msg.Length > 300)
					//	msg = msg.Substring(0, 300);

					//msg.ToToast(ToastNotificationType.Error, "Uh oh");
				}
				catch(Exception e)
				{
					Debug.WriteLine(e);
				}
			});
		}

		void SetDefaultPropertyValues()
		{
			PraisePhrases = new List<string> {
					"sensational",
					"crazmazing",
					"stellar",
					"ill",
					"peachy keen",
					"the bees knees",
					"the cat's pajamas",
					"the coolest kid in the cave",
					"killer",
					"aces",
					"spiffy",
					"wicked awesome",
					"kinda terrific",
					"top notch",
					"impressive",
					"legit",
					"nifty",
					"spectaculawesome",
					"supernacular",
					"bad to the bone",
					"radical",
					"neat",
					"a hoss boss",
					"mad chill",
					"super chill",
					"a beast",
					"funky fresh",
					"slammin it",
			};
		}
	}
}