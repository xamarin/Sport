using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace Sport.Mobile.Shared
{
	public interface IAuthenticator
	{
		Task<MobileServiceUser> Authenticate();
		void ClearCookies();
	}
}