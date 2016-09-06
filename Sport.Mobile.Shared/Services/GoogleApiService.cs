using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Sport.Mobile.Shared
{
	public class GoogleApiService
	{
		static GoogleApiService _instance;

		public static GoogleApiService Instance
		{
			get
			{
				return _instance ?? (_instance = new GoogleApiService());
			}
		}

		public Task<GoogleUserProfile> GetUserProfile(string token)
		{
			return new Task<GoogleUserProfile>(() =>
			{
				try
				{
					using(var client = new HttpClient())
					{
						const string url = "https://www.googleapis.com/oauth2/v1/userinfo?alt=json";
						client.DefaultRequestHeaders.Add("Authorization", token);
						var json = client.GetStringAsync(url).Result;
						var profile = JsonConvert.DeserializeObject<GoogleUserProfile>(json);
						return profile;
					}
				}
				catch(Exception e)
				{
					Debug.WriteLine(e);
					return null;
				}
			});
		}
	}
}