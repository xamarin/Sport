using Microsoft.WindowsAzure.MobileServices;

namespace Sport.Mobile.Shared
{
	public class Keys
	{
		public static readonly string SourceCodeUrl = "https://github.com/xamarin/sport";
		public static readonly string GooglePushNotificationSenderId = "643221174110";
        public static readonly string AzureDomainLocal = "http://10.211.55.3/Sport.Service/";
        public static readonly string AzureDomainRemote = "https://xamarin-sportv2.azurewebsites.net/";
        public static string AzureDomain = AzureDomainRemote;

		public static readonly string MobileCenterKeyiOS = "";
		public static readonly string MobileCenterKeyAndroid = "";
		public static readonly MobileServiceAuthenticationProvider AuthenticationProvider = MobileServiceAuthenticationProvider.Google;
    }
}