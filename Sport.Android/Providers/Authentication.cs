using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Sport.Shared;
using Xamarin;
using Xamarin.Forms;

[assembly: Dependency(typeof(Sport.Android.Authentication))]

namespace Sport.Android
{
	public class Authentication : IAuthentication
	{
		public async Task<MobileServiceUser> DisplayWebView()
		{
			try
			{
				return await AzureService.Instance.Client.LoginAsync(Forms.Context, MobileServiceAuthenticationProvider.Google);
			}
			catch(Exception e)
			{
				InsightsManager.Report(e);
			}

			return null;
		}

		public void ClearCookies()
		{
			global::Android.Webkit.CookieManager.Instance.RemoveAllCookies(null);
		}
	}
}