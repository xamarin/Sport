using System;
using System.Threading.Tasks;
using Foundation;
using Microsoft.WindowsAzure.MobileServices;
using Sport.Shared;
using Xamarin;
using Xamarin.Forms;

[assembly: Dependency(typeof(Sport.iOS.Authentication))]

namespace Sport.iOS
{
	public class Authentication : IAuthentication
	{
		public async Task<MobileServiceUser> DisplayWebView()
		{
			try
			{
				var window = UIKit.UIApplication.SharedApplication.KeyWindow;
				var root = window.RootViewController;
				if(root != null)
				{
					var current = root;
					while(current.PresentedViewController != null)
					{
						current = current.PresentedViewController;
					}

					return await AzureService.Instance.Client.LoginAsync(current, MobileServiceAuthenticationProvider.Google);
				}
			}
			catch(Exception e)
			{
				InsightsManager.Report(e);
			}

			return null;
		}

		public void ClearCookies()
		{
			var store = NSHttpCookieStorage.SharedStorage;
			var cookies = store.Cookies;

			foreach(var c in cookies)
			{
				store.DeleteCookie(c);
			}
		}
	}
}