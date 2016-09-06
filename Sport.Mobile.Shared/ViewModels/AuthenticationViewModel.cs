using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Sport.Mobile.Shared;
using Xamarin.Forms;
using System.Linq;

namespace Sport.Mobile.Shared
{
	public class AuthenticationViewModel : BaseViewModel
	{
		#region Properties

		IAuthenticator _authenticator = DependencyService.Get<IAuthenticator>();
		string _authenticationStatus;

		public string AuthenticationStatus
		{
			get
			{
				return _authenticationStatus;
			}
			set
			{
				SetPropertyChanged(ref _authenticationStatus, value);
			}
		}

		internal GoogleUserProfile AuthUserProfile
		{
			get;
			set;
		}

		#endregion

		/// <summary>
		/// Shows the Google authentication web view so the user can authenticate
		/// and gets the user's Google profile from Google API
		/// </summary>
		public async Task Authenticate()
		{
			using(new Busy(this))
			{
				if (!string.IsNullOrEmpty(Settings.AzureUserId))
				{
					//We already have an exsting google auth token that is still valid, no need to do anything else
					AzureService.Instance.Client.CurrentUser = new MobileServiceUser(Settings.AzureUserId)
					{
						MobileServiceAuthenticationToken = Settings.AzureAuthToken
					};

					var success = await GetUserProfile();

					if (success)
						return;
				}

				try
				{
					AuthenticationStatus = "Loading...";
					MobileServiceUser user = await _authenticator.Authenticate();
					await SetIdentityValues(user);
					await GetUserProfile();
				}
				catch (Exception e)
				{
					Debug.WriteLine("**SPORT AUTHENTICATION ERROR**\n\n" + e.GetBaseException());
					//InsightsManager.Report(e);
				}
			}
		}

		async Task SetIdentityValues(MobileServiceUser user)
		{
			var identity = await AzureService.Instance.Client.InvokeApiAsync("/.auth/me");

			Settings.GoogleAccessToken = identity[0].Value<string>("access_token");
			Settings.GoogleRefreshToken = identity[0].Value<string>("refresh_token");
			Settings.AzureUserId = user.UserId;
			Settings.AzureAuthToken = user.MobileServiceAuthenticationToken;
		}

		/// <summary>
		/// Authenticates the athlete against the Azure backend and loads all necessary data to begin the app experience
		/// </summary>
		async Task<bool> AuthenticateWithAzure()
		{
			Athlete athlete;
			AuthenticationStatus = "Getting athlete's profile";
			athlete = await GetAthletesProfile();

			if(athlete == null)
			{
				//Unable to get athlete - try registering as a new athlete
				athlete = await RegisterAthlete(AuthUserProfile);
			}
			else
			{
				athlete.ProfileImageUrl = AuthUserProfile.Picture;

				if(athlete.IsDirty)
				{
					await AzureService.Instance.AthleteManager.UpsertAsync(athlete);
				}
			}

			Settings.AthleteId = athlete?.Id;
			App.Instance.CurrentAthlete = athlete;

			if(App.Instance.CurrentAthlete != null)
			{
				await GetAllLeaderboards();
				MessagingCenter.Send(this, Messages.UserAuthenticated);
			}
			else
			{
			}

			AuthenticationStatus = "Done";
			return App.Instance.CurrentAthlete != null;
		}

		/// <summary>
		/// Gets the athlete's profile from the Azure backend
		/// </summary>
		async Task<Athlete> GetAthletesProfile()
		{
			Athlete athlete = null;

			//Let's try to load based on email address
			if(athlete == null && AuthUserProfile != null && !AuthUserProfile.Email.IsEmpty())
			{
				var task = AzureService.Instance.AthleteManager.GetAthleteByEmail(AuthUserProfile.Email);
				await RunSafe(task);

				if(task.IsCompleted && !task.IsFaulted)
					athlete = task.Result;
			}

			return athlete;
		}


		/// <summary>
		/// Registers an athlete with the backend and returns the new athlete profile
		/// </summary>
		async Task<Athlete> RegisterAthlete(GoogleUserProfile profile)
		{
			AuthenticationStatus = "Registering athlete";
			var athlete = new Athlete(profile);

			await AzureService.Instance.AthleteManager.UpsertAsync(athlete);

			"You're now an officially registered athlete!".ToToast();
			return athlete;
		}

		/// <summary>
		/// Gets all leaderboards for the system
		/// </summary>
		async Task GetAllLeaderboards()
		{
			AuthenticationStatus = "Getting leaderboards";
			App.Instance.Theming.UsedLeagueColors.Clear();
			await AzureService.Instance.SyncAllAsync();
		}

		/// <summary>
		/// Attempts to get the user profile from Google. Will use the refresh token if the auth token has expired
		/// Will set App.CurrentAthlete to the athlete data from Azure
		/// </summary>
		async public Task<bool> GetUserProfile()
		{
			//Can't get profile w/out a token
			if(Settings.GoogleAccessToken == null)
				return false;

			if(AuthUserProfile != null)
				return true;

			AuthenticationStatus = "Getting Google user profile";
			var task = GoogleApiService.Instance.GetUserProfile(Settings.AuthTokenAndType);
			await RunSafe(task, false);

			if(task.IsCompleted && (task.IsFaulted || task.Result == null))
			{
				//Need to refresh the token
				var refreshedUser = await AzureService.Instance.Client.RefreshUserAsync();
				await SetIdentityValues(refreshedUser);
				task = GoogleApiService.Instance.GetUserProfile(Settings.AuthTokenAndType);
				await RunSafe(task, false);
			}

			if(task.IsCompleted && !task.IsFaulted && task.Result != null)
			{
				AuthenticationStatus = "Authentication complete";
				AuthUserProfile = task.Result;

				//InsightsManager.Identify(AuthUserProfile.Email, new Dictionary<string, string> {
				//	{
				//		"Name",
				//		AuthUserProfile.Name
				//	}
				//});

				Settings.GoogleUserId = AuthUserProfile.Id;
			}
			else
			{
				AuthenticationStatus = "Unable to authenticate";
			}

			if(AuthUserProfile != null && App.Instance.CurrentAthlete == null)
			{
				await AuthenticateWithAzure();
			}

			return AuthUserProfile != null;
		}

		public void LogOut(bool clearCookies)
		{
			//Utility.SetSecured("AuthToken", string.Empty, "xamarin.sport", "authentication");
			//AzureService.Instance.Client.Logout();

			AuthUserProfile = null;
			Settings.GoogleAccessToken = null;
			Settings.AthleteId = null;
			Settings.GoogleUserId = null;

			if(clearCookies)
			{
				Settings.RegistrationComplete = false;
				_authenticator.ClearCookies();
			}
		}
	}
}