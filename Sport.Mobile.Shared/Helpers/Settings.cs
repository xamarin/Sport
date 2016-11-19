using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace Sport.Mobile.Shared
{
	public static class Settings
	{
		static ISettings AppSettings
		{
			get
			{
				return CrossSettings.Current;
			}
		}

		#region Setting Constants

		const string _googleUserId = "googleUserId";
		const string _athleteId = "athleteId";
		const string _deviceToken = "deviceToken";
		const string _googleRefreshToken = "googleRefreshToken";
		const string _googleAccessToken = "googleAccessToken";
		const string _registrationComplete = "registrationComplete";
		const string _azureUserId = "azureUserId";
		const string _azureAuthToken = "azureAuthToken";
		const string _enablePushNotifications = "enablePushNotifications";

		#endregion

		public static bool RegistrationComplete
		{
			get
			{
				return AppSettings.GetValueOrDefault<bool>(_registrationComplete, false);
			}
			set
			{
				AppSettings.AddOrUpdateValue(_registrationComplete, value);
			}
		}

		public static string DeviceToken
		{
			get
			{
				return AppSettings.GetValueOrDefault<string>(_deviceToken, null);
			}
			set
			{
				AppSettings.AddOrUpdateValue(_deviceToken, value);
			}
		}

		public static string RefreshToken
		{
			get
			{
				return AppSettings.GetValueOrDefault<string>(_googleRefreshToken, null);
			}
			set
			{
				AppSettings.AddOrUpdateValue(_googleRefreshToken, value);
			}
		}

		public static string AccessToken
		{
			get
			{
				return AppSettings.GetValueOrDefault<string>(_googleAccessToken, null);
			}
			set
			{
				AppSettings.AddOrUpdateValue(_googleAccessToken, value);
			}
		}

		public static string AccessTokenAndType
		{
			get
			{
				return AccessToken == null ? null : $"Bearer {AccessToken}";
			}
		}

		public static string AthleteId
		{
			get
			{
				return AppSettings.GetValueOrDefault<string>(_athleteId, null);
			}
			set
			{
				AppSettings.AddOrUpdateValue(_athleteId, value);
			}
		}

		public static string ProviderUserId
		{
			get
			{
				return AppSettings.GetValueOrDefault<string>(_googleUserId, null);
			}
			set
			{
				AppSettings.AddOrUpdateValue(_googleUserId, value);
			}
		}

		public static string AzureUserId
		{
			get
			{
				return AppSettings.GetValueOrDefault<string>(_azureUserId, null);
			}
			set
			{
				AppSettings.AddOrUpdateValue(_azureUserId, value);
			}
		}

		public static string AzureAuthToken
		{
			get
			{
				return AppSettings.GetValueOrDefault<string>(_azureAuthToken, null);
			}
			set
			{
				AppSettings.AddOrUpdateValue(_azureAuthToken, value);
			}
		}

		public static bool EnablePushNotifications
		{
			get
			{
				return AppSettings.GetValueOrDefault(_enablePushNotifications, false);
			}
			set
			{
				AppSettings.AddOrUpdateValue(_enablePushNotifications, value);
			}
		}
	}
}