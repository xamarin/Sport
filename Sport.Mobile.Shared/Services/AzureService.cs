using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using ModernHttpClient;
using System.Diagnostics;

namespace Sport.Mobile.Shared
{
	public class AzureService
	{
		public AzureService()
		{
			var url = new Uri(Keys.AzureDomain);
			var store = new MobileServiceSQLiteStore($"{url.Host}.db");
			store.DefineTable<Athlete>();
			store.DefineTable<League>();
			store.DefineTable<Membership>();
			store.DefineTable<Challenge>();
			store.DefineTable<GameResult>();
			Client.SyncContext.InitializeAsync(store);

			LeagueManager = new LeagueManager();
			MembershipManager = new MembershipManager();
			AthleteManager = new AthleteManager();
			ChallengeManager = new ChallengeManager();
			GameResultManager = new GameResultManager();
		}

		#region Properties

		public GameResultManager GameResultManager
		{
			get;
			private set;
		}

		public AthleteManager AthleteManager
		{
			get;
			private set;
		}

		public MembershipManager MembershipManager
		{
			get;
			private set;
		}

		public ChallengeManager ChallengeManager
		{
			get;
			private set;
		}

		public LeagueManager LeagueManager
		{
			get;
			private set;
		}


		static AzureService _instance;

		public static AzureService Instance
		{
			get
			{
				return _instance ?? (_instance = new AzureService());
			}
		}

		MobileServiceClient _client;

		public MobileServiceClient Client
		{
			get
			{
				if(_client == null)
				{
					var handler = new NativeMessageHandler();
					_client = new MobileServiceClient(Keys.AzureDomain, new HttpMessageHandler[] {
						handler,
					});
				}

				_client.AlternateLoginHost = new Uri(Keys.AzureDomainRemote);
				return _client;
			}			
		}

		#endregion

		public async Task<bool> SyncAllAsync()
		{
			var list = new List<Task<bool>>();

			list.Add(GameResultManager.SyncAsync());
			list.Add(ChallengeManager.SyncAsync());
			list.Add(MembershipManager.SyncAsync());
			list.Add(AthleteManager.SyncAsync());
			list.Add(LeagueManager.SyncAsync());

			var successes = await Task.WhenAll(list).ConfigureAwait(false);
			var count = MembershipManager.Table.ToListAsync().Result;
			return successes.Any(x => !x);
		}

		#region Push Notifications

		/// <summary>
		/// This app uses Azure as the backend which utilizes Notifications hubs
		/// </summary>
		/// <returns>The athlete notification hub registration.</returns>
		public Task UpdateAthleteNotificationHubRegistration(Athlete athlete, bool forceSave = false, bool sendTestPush = false)
		{
			return new Task(() =>
			{
				if(athlete == null)
					throw new ArgumentNullException(nameof(Athlete));

				if(athlete.Id == null || athlete.DeviceToken == null)
					return;

				var tags = new List<string> {
					athlete.Id,
					"All",
				};

				App.Instance.CurrentAthlete.LocalRefresh();
				App.Instance.CurrentAthlete.Memberships.Select(m => m.LeagueId).ToList().ForEach(tags.Add);
				athlete.DevicePlatform = Xamarin.Forms.Device.OS.ToString();

				var reg = new DeviceRegistration {
					Handle = athlete.DeviceToken,
					Platform = athlete.DevicePlatform,
					Tags = tags.ToArray()
				};

				var registrationId = Client.InvokeApiAsync<DeviceRegistration, string>("registerWithHub", reg, HttpMethod.Put, null).Result;
				athlete.NotificationRegistrationId = registrationId;

				if(athlete.IsDirty || forceSave)
				{
					var success = AthleteManager.UpsertAsync(athlete).Result;
				}

				//Used to verify the device is successfully registered with the backend 
				if(sendTestPush)
				{
					var qs = new Dictionary<string, string>();
					qs.Add("athleteId", athlete.Id);
					Client.InvokeApiAsync("sendTestPushNotification", null, HttpMethod.Get, qs).Wait();
				}
			});
		}

		public Task UnregisterAthleteForPush(Athlete athlete)
		{
			return new Task(() =>
			{
				if(athlete == null || athlete.NotificationRegistrationId == null)
					return;

				var values = new Dictionary<string, string> { {
						"id",
						athlete.NotificationRegistrationId
					}
				};
				var registrationId = Client.InvokeApiAsync<string>("unregister", HttpMethod.Delete, values).Result;
			});
		}

		#endregion
	}
}