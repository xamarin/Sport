using System;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace Sport.Shared
{
	public class GoogleApiService
	{
		#region Properties

		static GoogleApiService _instance;

		public static GoogleApiService Instance
		{
			get
			{
				return _instance ?? (_instance = new GoogleApiService());
			}
		}

		#endregion

		#region Authentication

		public Task<GoogleUserProfile> GetUserProfile()
		{
			return new Task<GoogleUserProfile>(() =>
			{
				using(var client = new HttpClient())
				{
					const string url = "https://www.googleapis.com/oauth2/v1/userinfo?alt=json";
					client.DefaultRequestHeaders.Add("Authorization", App.AuthTokenAndType);
					var json = client.GetStringAsync(url).Result;
					var profile = JsonConvert.DeserializeObject<GoogleUserProfile>(json);
					return profile;
				}
			});
		}

		#endregion
	}
}

