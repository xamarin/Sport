using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Foundation;
using Microsoft.WindowsAzure.MobileServices;
using Sport.Mobile.Shared;
using Xamarin.Forms;

[assembly: Dependency(typeof(Sport.Mobile.iOS.Authenticator))]

namespace Sport.Mobile.iOS
{
	public class Authenticator : IAuthenticator
	{
		public async Task<MobileServiceUser> Authenticate()
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

				var user = await AzureService.Instance.Client.LoginAsync(current, Keys.AuthenticationProvider, new Dictionary<string, string>() { { "access_type", "offline" } });
				return user;
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