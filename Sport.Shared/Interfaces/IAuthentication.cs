using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace Sport.Shared
{
	public interface IAuthentication
	{
		Task<MobileServiceUser> DisplayWebView();

		void ClearCookies();
	}
}