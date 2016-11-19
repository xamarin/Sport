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
			return await AzureService.Instance.Client.LoginAsync(Forms.Context, Keys.AuthenticationProvider, new Dictionary<string, string>() { { "access_type", "offline" } });
		}

		public void ClearCookies()
		{
			global::Android.Webkit.CookieManager.Instance.RemoveAllCookies(null);
		}
	}
}