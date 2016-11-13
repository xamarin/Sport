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

					var user = await AzureService.Instance.Client.LoginAsync(current, MobileServiceAuthenticationProvider.Google, new Dictionary<string, string>() { { "access_type", "offline" } });
					return user;
				}
			}
			catch(Exception e)
			{
				Debug.WriteLine(e);
				MessagingCenter.Send(new object(), Shared.Messages.ExceptionOccurred, e);
				//InsightsManager.Report(e);
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