using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Sport.Mobile.Shared;
using Xamarin.Forms;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Collections.Generic;

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

		internal IUserProfile AuthUserProfile
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

					bool clearCookies = true;
					bool didErr = false;
					try
					{
						var success = await GetUserProfile();

						if(success)
							return;
					}
					catch(MobileServiceInvalidOperationException mse)
					{
						//Bad or stale credentials, clear out and retry
						if(mse.Response.StatusCode == HttpStatusCode.Forbidden)
						{
							clearCookies = false;
						}
						didErr = true;
						Debug.WriteLine(mse);
					}
					catch(Exception e)
					{
						Debug.WriteLine(e);
						didErr = true;
					}

					if(didErr)
					{
						await LogOut(clearCookies);
						await Authenticate();
						return;
					}
				}

				try
				{
					AuthenticationStatus = "Loading...";
					MobileServiceUser user = await _authenticator.Authenticate();

					if(user != null)
					{
						await SetIdentityValues(user);
						await GetUserProfile();
					}
				}
				catch(InvalidOperationException ie)
				{
					//User cancelled
				}
				catch(Exception e)
				{
					MessagingCenter.Send(new object(), Messages.ExceptionOccurred, e);
				}
			}
		}

		AppServiceIdentity _identity;
		async Task SetIdentityValues(MobileServiceUser user)
		{
			//Manually calling /.auth/me against remote server because InvokeApiAsync against a local instance of Azure will never hit the remote endpoint
			//If you are not debugging your service locally, you can just use InvokeApiAsync("/.auth/me") 
			if(_identity == null)
			{
				using(var client = new HttpClient())
				{
					client.DefaultRequestHeaders.Add("ZUMO-API-VERSION", "2.0.0");
					client.DefaultRequestHeaders.Add("X-ZUMO-AUTH", AzureService.Instance.Client.CurrentUser.MobileServiceAuthenticationToken);
					var json = await client.GetStringAsync($"{Keys.AzureDomainRemote}/.auth/me");
					_identity = JsonConvert.DeserializeObject<List<AppServiceIdentity>>(json).FirstOrDefault();
				}
			}

			if(_identity != null)
			{
				Settings.AzureAuthToken = user.MobileServiceAuthenticationToken;
				Settings.AzureUserId = user.UserId;

				switch(Keys.AuthenticationProvider)
				{
					case MobileServiceAuthenticationProvider.Google:
						Settings.AccessToken = _identity.AccessToken;
						Settings.RefreshToken = _identity.RefreshToken;
						break;
						
					case MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory:
						break;
						
				}
			}
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
				athlete.ProfileImageUrl = AuthUserProfile.PhotoUrl;

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
		async Task<Athlete> RegisterAthlete(IUserProfile profile)
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
			if(AuthUserProfile != null)
				return true;

			AuthenticationStatus = "Getting user profile";
			var profile = await GetUserProfile(Keys.AuthenticationProvider);

			if(profile == null)
			{
				//Need to refresh the token
				try
				{
					var refreshedUser = await AzureService.Instance.Client.RefreshUserAsync();
					await SetIdentityValues(refreshedUser);
					profile = await GetUserProfile(Keys.AuthenticationProvider);
				}
				catch(MobileServiceInvalidOperationException ex)
				{
					Debug.WriteLine("Error refreshing token: " + ex);
					throw;
				}
			}

			if(profile != null)
			{
				AuthenticationStatus = "Authentication complete";
				AuthUserProfile = profile;

				//InsightsManager.Identify(AuthUserProfile.Email, new Dictionary<string, string> {
				//	{
				//		"Name",
				//		AuthUserProfile.Name
				//	}
				//});

				Settings.ProviderUserId = AuthUserProfile.Id;
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

		async public Task LogOut(bool clearCookies)
		{
			await AzureService.Instance.Client.LogoutAsync();

			App.Instance.CurrentAthlete = null;
			AuthUserProfile = null;
			Settings.AccessToken = null;
			Settings.RefreshToken = null;
			Settings.ProviderUserId = null;
			Settings.AzureUserId = null;
			Settings.AzureAuthToken = null;

			if(clearCookies)
			{
				Settings.RegistrationComplete = false;
				_authenticator.ClearCookies();
			}
		}

		async public Task<IUserProfile> GetUserProfile(MobileServiceAuthenticationProvider provider)
		{
			switch(provider)
			{
				case MobileServiceAuthenticationProvider.Google:
					try
					{
						//Can't get profile w/out a token
						if(Settings.AccessToken == null)
							return null;

						using(var client = new HttpClient())
						{
							const string url = "https://www.googleapis.com/oauth2/v1/userinfo?alt=json";
							client.DefaultRequestHeaders.Add("Authorization", Settings.AccessTokenAndType);
							var json = await client.GetStringAsync(url);
							var profile = JsonConvert.DeserializeObject<GoogleUserProfile>(json);
							return profile;
						}
					}
					catch(Exception e)
					{
						Debug.WriteLine(e);
						return null;
					}

				case MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory:

					if(_identity == null)
						return null;

					var ad = new ActiveDirectoryUserProfile();
					ad.Id = _identity.UserClaims.SingleOrDefault(c => c.Typ == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Val;
					ad.Email = _identity.UserClaims.SingleOrDefault(c => c.Typ == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Val;
					ad.Name = _identity.UserClaims.SingleOrDefault(c => c.Typ == "name")?.Val;

					//using(var client = new HttpClient())
					//{
					//	client.DefaultRequestHeaders.Add("ZUMO-API-VERSION", "2.0.0");
					//	client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AzureService.Instance.Client.CurrentUser.MobileServiceAuthenticationToken);
					//	client.DefaultRequestHeaders.Add("X-ZUMO-AUTH", AzureService.Instance.Client.CurrentUser.MobileServiceAuthenticationToken);
					//	var json = client.GetStringAsync("https://graph.microsoft.com/v1.0/me/").Result;
					//	Debug.WriteLine(json);
					//	//identity = JsonConvert.DeserializeObject<JToken>(json);
					//}

					return ad;

				default:
					return null;
			}
		}
	}
}