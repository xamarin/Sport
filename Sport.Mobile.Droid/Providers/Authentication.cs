using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Sport.Mobile.Shared;
using Xamarin.Forms;

[assembly: Dependency(typeof(Sport.Android.Authentication))]

namespace Sport.Android
{
	public class Authentication : IAuthenticator
	{
		public async Task<MobileServiceUser> Authenticate()
		{
			try
			{
				return await AzureService.Instance.Client.LoginAsync(Forms.Context, MobileServiceAuthenticationProvider.Google, new Dictionary<string, string>() { { "access_type", "offline" } });
			}
			catch(Exception e)
			{
				Debug.WriteLine(e);
				//InsightsManager.Report(e);
			}

			return null;
		}

		public void ClearCookies()
		{
			global::Android.Webkit.CookieManager.Instance.RemoveAllCookies(null);
		}
	}
}