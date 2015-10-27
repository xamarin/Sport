using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Sport.Shared;
using Xamarin;
using Xamarin.Forms;

[assembly: Dependency(typeof(AuthenticationViewModel))]

namespace Sport.Shared
{
	public class AuthenticationViewModel : BaseViewModel
	{
		#region Properties

		IAuthentication _authenticator = DependencyService.Get<IAuthentication>();
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
		/// Performs a complete authentication pass
		/// </summary>
		public async Task<bool> AuthenticateCompletely()
		{
			await AuthenticateWithGoogle();

			if(AuthUserProfile != null)
				await AuthenticateWithBackend();

			return App.CurrentAthlete != null;
		}

		/// <summary>
		/// Attempts to get the user's profile and will use WebView form to authenticate if necessary
		/// </summary>
		async Task<bool> AuthenticateWithGoogle()
		{
			await ShowGoogleAuthenticationView();

			if(App.AuthToken == null)
				return false;

			await GetUserProfile();
			return AuthUserProfile != null;
		}

		/// <summary>
		/// Shows the Google authentication web view so the user can authenticate
		/// </summary>
		async Task ShowGoogleAuthenticationView()
		{
			if(App.AuthToken != null && Settings.Instance.User != null)
			{
				var success = await GetUserProfile();

				if(success)
				{
					AzureService.Instance.Client.CurrentUser = Settings.Instance.User;
					return;
				}
			}

			try
			{
				AuthenticationStatus = "Loading...";
				MobileServiceUser user = await _authenticator.DisplayWebView();

				var identity = await AzureService.Instance.Client.InvokeApiAsync("getUserIdentity", null, HttpMethod.Get, null);

				App.AuthToken = identity.Value<string>("accessToken");
				Utility.SetSecured("AuthToken", App.AuthToken, "xamarin.sport", "authentication");

				Settings.Instance.User = user;
				await Settings.Instance.Save();

				if(App.CurrentAthlete != null && App.CurrentAthlete.Id != null)
				{
					var task = AzureService.Instance.SaveAthlete(App.CurrentAthlete);
					await RunSafe(task);
				}
			}
			catch(Exception e)
			{
				Console.WriteLine("**SPORT AUTHENTICATION ERROR**\n\n" + e.GetBaseException());
				InsightsManager.Report(e);
			}
		}

		/// <summary>
		/// Authenticates the athlete against the Azure backend and loads all necessary data to begin the app experience
		/// </summary>
		async Task<bool> AuthenticateWithBackend()
		{
			Athlete athlete;
			using(new Busy(this))
			{
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
						var task = AzureService.Instance.SaveAthlete(athlete);
						await RunSafe(task);
					}
				}

				Settings.Instance.AthleteId = athlete?.Id;
				await Settings.Instance.Save();

				if(App.CurrentAthlete != null)
				{
					await GetAllLeaderboards();
					App.CurrentAthlete.IsDirty = false;
					MessagingCenter.Send<AuthenticationViewModel>(this, Messages.UserAuthenticated);
				}

				AuthenticationStatus = "Done";
				return App.CurrentAthlete != null;
			}
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
				var task = AzureService.Instance.GetAthleteByEmail(AuthUserProfile.Email);
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

			var task = AzureService.Instance.SaveAthlete(athlete);
			await RunSafe(task);

			if(task.IsCompleted && task.IsFaulted)
				return null;

			"You're now an officially registered athlete!".ToToast();
			return athlete;
		}

		/// <summary>
		/// Gets all leaderboards for the system
		/// </summary>
		async Task GetAllLeaderboards()
		{
			AuthenticationStatus = "Getting leaderboards";
			var task = AzureService.Instance.GetAllLeaguesForAthlete(App.CurrentAthlete);
			await RunSafe(task);

			if(task.IsCompleted && !task.IsFaulted)
			{
				App.Current.UsedLeagueColors.Clear();
				task.Result.EnsureLeaguesThemed();
			}
		}

		/// <summary>
		/// Attempts to get the user profile from Google. Will use the refresh token if the auth token has expired
		/// </summary>
		async public Task<bool> GetUserProfile()
		{
			//Can't get profile w/out a token
			if(App.AuthToken == null)
				return false;

			if(AuthUserProfile != null)
				return true;

			using(new Busy(this))
			{
				AuthenticationStatus = "Getting Google user profile";
				var task = GoogleApiService.Instance.GetUserProfile();
				await RunSafe(task, false);

				if(task.IsFaulted && task.IsCompleted)
				{
					//Need to get refresh token from Azure somehow
					//Likely our authtoken has expired
//					AuthenticationStatus = "Refreshing token";
//
//					var refreshTask = GoogleApiService.Instance.GetNewAuthToken(Settings.Instance.RefreshToken);
//					await RunSafe(refreshTask);
//
//					if(refreshTask.IsCompleted && !refreshTask.IsFaulted)
//					{
//						//Success in getting a new auth token - now lets attempt to get the profile again
//						if(!string.IsNullOrWhiteSpace(refreshTask.Result) && App.AuthToken != refreshTask.Result)
//						{
//							//We have a valid token now, let's try this again
//							App.AuthToken = refreshTask.Result;
//							await Settings.Instance.Save();
//							return await GetUserProfile();
//						}
//					}
				}

				if(task.IsCompleted && !task.IsFaulted && task.Result != null)
				{
					AuthenticationStatus = "Authentication complete";
					AuthUserProfile = task.Result;

					InsightsManager.Identify(AuthUserProfile.Email, new Dictionary<string, string> {
						{
							"Name",
							AuthUserProfile.Name
						}
					});

					Settings.Instance.AuthUserID = AuthUserProfile.Id;
					await Settings.Instance.Save();
				}
				else
				{
					AuthenticationStatus = "Unable to authenticate";
				}
			}

			return AuthUserProfile != null;
		}

		public void LogOut(bool clearCookies)
		{
			Utility.SetSecured("AuthToken", string.Empty, "xamarin.sport", "authentication");
			AzureService.Instance.Client.Logout();

			App.AuthToken = null;
			AuthUserProfile = null;
			Settings.Instance.AthleteId = null;
			Settings.Instance.AuthUserID = null;

			if(clearCookies)
			{
				Settings.Instance.RegistrationComplete = false;
				_authenticator.ClearCookies();
			}

			Settings.Instance.Save();
		}
	}
}