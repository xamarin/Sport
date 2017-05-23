using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Foundation;
using Microsoft.WindowsAzure.MobileServices;
using Sport.Mobile.Shared;
using Xamarin.Forms;
using SimpleAuth;
using SimpleAuth.Providers;
using Newtonsoft.Json.Linq;
using System.Net.Http;

[assembly: Dependency (typeof (Sport.Mobile.iOS.Authenticator))]

namespace Sport.Mobile.iOS
{
	public class Authenticator : IAuthenticator
	{
		public async Task<MobileServiceUser> Authenticate ()
		{
			Account account = null;

			try {
				var scopes = new []
				{
					"https://www.googleapis.com/auth/userinfo.email",
					"https://www.googleapis.com/auth/userinfo.profile",
				};

				const string clientId = "381689253740-p0m6strndvpajfqe2o5ia1b3si075snn.apps.googleusercontent.com";
				const string serverID = "381689253740-2scljkrh5i7fjfvsuh438cde54hfoo5i.apps.googleusercontent.com";

				var api = new GoogleApi ("google",clientId ) {
					ServerClientId = serverID,
					Scopes = scopes,
				};
				api.ResetData ();
				account = await api.Authenticate ();
				var oauth = account as OAuthAccount;
				var token = account.UserData ["ServerToken"];
				if (account != null) {
					var jObject = JObject.Parse ($"{{'id_token':'{oauth.IdToken}', 'authorization_code':'{token}'}}");
					var usr = await AzureService.Instance.Client.LoginAsync (MobileServiceAuthenticationProvider.Google, jObject);
					return usr;

				}

			} catch (MobileServiceInvalidOperationException e) {
				Console.WriteLine (e);
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}

			return null;
		}

		public void ClearCookies ()
		{
			
		}
	}
}

//public class AzureGoogleApi : GoogleApi
//{
//	public AzureGoogleApi (string identifier, string clientId, HttpMessageHandler handler = null) : base (identifier, clientId, handler)
//	{

//	}

//	public AzureGoogleApi (string identifier, string clientId, string clientSecret, HttpMessageHandler handler = null) : base (identifier, clientId, clientSecret, handler)
//	{

//	}

//	protected override async Task<OAuthAccount> GetAccountFromAuthCode (WebAuthenticator authenticator, string identifier)
//	{
//		var account = await base.GetAccountFromAuthCode (authenticator, identifier);
//		account.UserData ["AuthCode"] = authenticator.AuthCode;
//		return account;

//	}
//}