using System;
using Refractored.Xam.Settings;
using Refractored.Xam.Settings.Abstractions;

namespace SportChallengeMatchRank.Shared
{
	public class AppSettings
	{
		private static ISettings Instance
		{
			get
			{
				return CrossSettings.Current;
			}
		}

		const string _authTokenKey = "auth_token";

		public static string AuthToken
		{
			get
			{
				return Instance.GetValueOrDefault<string>(_authTokenKey);
			}
			set
			{
				Instance.AddOrUpdateValue(_authTokenKey, value);
			}
		}

		const string _authUserIdKey = "user_id";

		public static string AuthUserID
		{
			get
			{
				return Instance.GetValueOrDefault<string>(_authUserIdKey);
			}
			set
			{
				Instance.AddOrUpdateValue(_authUserIdKey, value);
			}
		}

		const string _athleteIdKey = "athlete_id";

		public static string AthleteId
		{
			get
			{
				return Instance.GetValueOrDefault<string>(_athleteIdKey);
			}
			set
			{
				Instance.AddOrUpdateValue(_athleteIdKey, value);
			}
		}
	}
}