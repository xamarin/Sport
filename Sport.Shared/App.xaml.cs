using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Connectivity;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]

namespace Sport.Shared
{
	public partial class App : Application
	{
		#region Properties

		public IHUDProvider _hud;
		public static int AnimationSpeed = 250;

		public static NotificationPayload PendingNotificationPayload
		{
			get;
			private set;
		}

		public IHUDProvider Hud
		{
			get
			{
				return _hud ?? (_hud = DependencyService.Get<IHUDProvider>());
			}
		}

		public new static App Current
		{
			get
			{
				return (App)Application.Current;
			}
		}

		public static Athlete CurrentAthlete
		{
			get
			{
				return Settings.Instance.AthleteId == null ? null : DataManager.Instance.Athletes.Get(Settings.Instance.AthleteId);
			}
		}

		public static bool IsNetworkRechable
		{
			get;
			set;
		}

		public static List<string> PraisePhrases
		{
			get;
			set;
		}

		public static List<string> AvailableLeagueColors
		{
			get;
			set;
		}

		public Dictionary<string, string> UsedLeagueColors
		{
			get;
			set;
		} = new Dictionary<string, string>();

		public static string AuthToken
		{
			get;
			set;
		}

		public static string AuthTokenAndType
		{
			get
			{
				return AuthToken == null ? null : "{0} {1}".Fmt("Bearer", AuthToken);
			}
		}

		#endregion

		public App()
		{
			try
			{
				App.AuthToken = Utility.GetSecured("AuthToken", "xamarin.sport", "authentication", null);
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}

			SetDefaultPropertyValues();

			InitializeComponent();
			MessagingCenter.Subscribe<BaseViewModel, Exception>(this, "ExceptionOccurred", OnAppExceptionOccurred);
			IsNetworkRechable = CrossConnectivity.Current.IsConnected;

			CrossConnectivity.Current.ConnectivityChanged += (sender, args) =>
			{
				IsNetworkRechable = args.IsConnected;
			};

			if(Settings.Instance.AthleteId == null || !Settings.Instance.RegistrationComplete)
			{
				StartRegistrationFlow();
			}
			else
			{
				StartAuthenticationFlow();
			}
		}

		protected override void OnSleep()
		{
			MessagingCenter.Unsubscribe<App, NotificationPayload>(this, Messages.IncomingPayloadReceivedInternal);
			base.OnSleep();
		}

		protected override void OnStart()
		{
			MessagingCenter.Subscribe<App, NotificationPayload>(this, Messages.IncomingPayloadReceivedInternal, (sender, payload) => OnIncomingPayload(payload));
			base.OnStart();
		}

		/// <summary>
		/// Kicks off the main application flow - this is the typical route taken once a user is registered
		/// </summary>
		void StartAuthenticationFlow()
		{
			//Create our entry page and add it to a NavigationPage, then apply a randomly assigned color theme
			var page = new AthleteLeaguesPage(App.CurrentAthlete?.Id);
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

		/// <summary>
		/// All application exceptions should be routed through this method so they get process/displayed to the user in a consistent manner
		/// </summary>
		void OnAppExceptionOccurred(BaseViewModel viewModel, Exception exception)
		{
			Device.BeginInvokeOnMainThread(async() =>
			{
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

					if(msg.Length > 300)
						msg = msg.Substring(0, 300);

					msg.ToToast(ToastNotificationType.Error, "Uh oh");
				}
				catch(Exception e)
				{
					Debug.WriteLine(e);
				}
			});
		}

		/// <summary>
		/// This method is here purely to handle shelved push notifications
		/// </summary>
		/// <param name="payload">Payload.</param>
		internal async Task OnIncomingPayload(NotificationPayload payload)
		{
			if(payload == null)
				return;

			if(App.CurrentAthlete == null)
			{
				PendingNotificationPayload = payload;
			}
			else
			{
				MessagingCenter.Send<App, NotificationPayload>(App.Current, Messages.IncomingPayloadReceived, payload);
			}
		}

		internal void ProcessPendingPayload()
		{
			if(PendingNotificationPayload == null)
				return;
			
			MessagingCenter.Send<App, NotificationPayload>(App.Current, Messages.IncomingPayloadReceived, PendingNotificationPayload);
			PendingNotificationPayload = null;
		}

		#region Theme

		/// <summary>
		/// Assigns a league a randomly-chosen theme from an existing finite list
		/// </summary>
		/// <returns>The theme.</returns>
		public ColorTheme GetTheme(League league)
		{
			if(league.Id == null)
				return null;

			league.Theme = null;
			var remaining = App.AvailableLeagueColors.Except(App.Current.UsedLeagueColors.Values).ToList();
			if(remaining.Count == 0)
				remaining.AddRange(App.AvailableLeagueColors);

			var random = new Random().Next(0, remaining.Count - 1);
			var color = remaining[random];

			if(App.Current.UsedLeagueColors.ContainsKey(league.Id))
			{
				color = App.Current.UsedLeagueColors[league.Id];
			}
			else
			{
				App.Current.UsedLeagueColors.Add(league.Id, color);
			}

			var theme = GetThemeFromColor(color);

			if(App.Current.Resources.ContainsKey("{0}Medium".Fmt(color)))
				theme.Medium = (Color)App.Current.Resources["{0}Medium".Fmt(color)];

			return theme;
		}

		public ColorTheme GetThemeFromColor(string color)
		{
			return new ColorTheme {
				Primary = (Color)App.Current.Resources["{0}Primary".Fmt(color)],
				Light = (Color)App.Current.Resources["{0}Light".Fmt(color)],
				Dark = (Color)App.Current.Resources["{0}Dark".Fmt(color)],
			};
		}

		void SetDefaultPropertyValues()
		{
			AvailableLeagueColors = new List<string> {
					"red",
					"green",
					"blue",
					"asphalt",
					"yellow",
					"purple"
			};

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

		#endregion
	}

	#region LeagueTheme

	public class ColorTheme
	{
		public Color Primary
		{
			get;
			set;
		}

		public Color Light
		{
			get;
			set;
		}

		public Color Dark
		{
			get;
			set;
		}

		public Color Medium
		{
			get;
			set;
		}

		public Color PrimaryText
		{
			get;
			set;
		}
	}

	#endregion
}