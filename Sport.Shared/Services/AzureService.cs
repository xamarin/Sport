using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using ModernHttpClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sport.Shared
{
	public class AzureService
	{
		#region Properties

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

					#if __IOS__

					//Use ModernHttpClient for caching and to allow traffic to be routed through Charles/Fiddler/etc
					handler = new ModernHttpClient.NativeMessageHandler() {
						Proxy = CoreFoundation.CFNetwork.GetDefaultProxy(),
						UseProxy = true,
					};

					#endif

					_client = new MobileServiceClient(Keys.AzureDomain, Keys.AzureApplicationKey, new HttpMessageHandler[] {
						new LeagueExpandHandler(),
						new ChallengeExpandHandler(),
						handler,
					});

					CurrentPlatform.Init();
				}

				return _client;
			}			
		}

		#endregion

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
					throw new ArgumentNullException("athlete");

				if(athlete.Id == null || athlete.DeviceToken == null)
					return;

				var tags = new List<string> {
					App.CurrentAthlete.Id,
					"All",
				};

				App.CurrentAthlete.LocalRefresh();
				App.CurrentAthlete.Memberships.Select(m => m.LeagueId).ToList().ForEach(tags.Add);
				athlete.DevicePlatform = Xamarin.Forms.Device.OS.ToString();

				var reg = new DeviceRegistration {
					Handle = athlete.DeviceToken,
					Platform = athlete.DevicePlatform,
					Tags = tags.ToArray()
				};

				var registrationId = Client.InvokeApiAsync<DeviceRegistration, string>("registerWithHub", reg, HttpMethod.Put, null).Result;
				athlete.NotificationRegistrationId = registrationId;

				//Used to verify the device is successfully registered with the backend 
				if(sendTestPush)
				{
					var qs = new Dictionary<string, string>();
					qs.Add("athleteId", athlete.Id);
					Client.InvokeApiAsync("sendTestPushNotification", null, HttpMethod.Get, qs).Wait();
				}

				if(athlete.IsDirty || forceSave)
				{
					var task = SaveAthlete(athlete);
					task.Start();
					task.Wait();
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

		#region League

		public Task<List<League>> GetAllLeagues()
		{
			return new Task<List<League>>(() =>
			{
				DataManager.Instance.Leagues.Clear();
				var list = Client.GetTable<League>().OrderBy(l => l.Name).ToListAsync().Result;
				list.ForEach(l => DataManager.Instance.Leagues.AddOrUpdate(l));
				return list;
			});
		}

		public Task<List<League>> GetAvailableLeagues(Athlete athlete)
		{
			return new Task<List<League>>(() =>
			{
				var memberOf = athlete.Memberships.Select(m => m.LeagueId).ToList();
				var existingToJoin = DataManager.Instance.Leagues.Keys.Except(memberOf);
					
				List<League> list;

				if(memberOf.Count > 0)
				{
					list = Client.GetTable<League>().Where(l => !memberOf.Contains(l.Id) && l.IsEnabled && l.IsAcceptingMembers).OrderBy(l => l.Name).ToListAsync().Result;
				}
				else
				{
					list = Client.GetTable<League>().Where(l => l.IsEnabled && l.IsAcceptingMembers).OrderBy(l => l.Name).ToListAsync().Result;
				}

				var toRemoveFromCache = existingToJoin.Except(list.Select(l => l.Id)).ToList();

				League removed;
				toRemoveFromCache.ForEach(l => DataManager.Instance.Leagues.TryRemove(l, out removed));

				EnsureAthletesLoadedForLeagues(list);
				list.ForEach(CacheLeague);
				return list;
			});
		}

		public Task GetAllAthletesForLeague(League league)
		{
			return new Task(() =>
			{
				var memberships = Client.GetTable<Membership>().Where(m => m.LeagueId == league.Id && m.AbandonDate == null).OrderBy(m => m.CurrentRank).ToListAsync().Result;
	
				var qs = new Dictionary<string, string>();
				qs.Add("leagueId", league.Id);
				var athletes = Client.InvokeApiAsync<string, List<Athlete>>("getAthletesForLeague", null, HttpMethod.Get, qs).Result;

				foreach(var m in DataManager.Instance.Memberships.Values.Where(m => m.LeagueId == league.Id).ToList())
				{
					Membership mem;
					DataManager.Instance.Memberships.TryRemove(m.Id, out mem);
				}

				foreach(var m in memberships)
				{
					var athlete = athletes.SingleOrDefault(a => a.Id == m.AthleteId);
					athlete = athlete ?? DataManager.Instance.Athletes.Get(m.AthleteId);

					if(athlete == null)
					{
						DeleteMembership(m.Id).Wait();
						continue;
					}

					DataManager.Instance.Memberships.AddOrUpdate(m);
					DataManager.Instance.Athletes.AddOrUpdate(athlete);
					m.SetPropertyChanged("Athlete");
				}

				DataManager.Instance.Athletes.Values.ToList().ForEach(a => a.LocalRefresh());
				DataManager.Instance.Leagues.Values.ToList().ForEach(l => l.LocalRefresh());
				athletes.ForEach(a => a.IsDirty = false);
			});
		}

		public Task<League> GetLeagueById(string id, bool force = false)
		{
			return new Task<League>(() =>
			{
				League a = null;

				if(!force)
					DataManager.Instance.Leagues.TryGetValue(id, out a);

				if(a == null)
				{
					a = Client.GetTable<League>().LookupAsync(id).Result;

					if(!a.IsEnabled)
						return null;
					
					CacheLeague(a);
				}

				return a;
			});
		}

		public Task SaveLeague(League league)
		{
			return new Task(() =>
			{
				if(league.Id == null)
				{
					Client.GetTable<League>().InsertAsync(league).Wait();
				}
				else
				{
					Client.GetTable<League>().UpdateAsync(league).Wait();
				}

				DataManager.Instance.Leagues.AddOrUpdate(league);
			});
		}

		public Task DeleteLeague(string id)
		{
			return new Task(() =>
			{
				League l;
				try
				{
					Client.GetTable<League>().DeleteAsync(new League {
						Id = id
					}).Wait();
					DataManager.Instance.Leagues.TryRemove(id, out l);
				}
				catch(HttpRequestException hre)
				{
					if(hre.Message.ContainsNoCase("not found"))
					{
						DataManager.Instance.Leagues.TryRemove(id, out l);
					}
				}
				catch(Exception e)
				{
					Debug.WriteLine(e);
				}
			});
		}

		#endregion

		#region Athlete

		public Task<List<Athlete>> GetAllAthletes()
		{
			return new Task<List<Athlete>>(() =>
			{
				DataManager.Instance.Athletes.Clear();
				var list = Client.GetTable<Athlete>().OrderBy(a => a.Name).ToListAsync().Result;
				list.ForEach(a => DataManager.Instance.Athletes.AddOrUpdate(a));
				return list;
			});
		}

		public Task<Athlete> GetAthleteByEmail(string email)
		{
			return new Task<Athlete>(() =>
			{
				var list = Client.GetTable<Athlete>().Where(a => a.Email == email).ToListAsync().Result;
				var athlete = list.FirstOrDefault();

				if(athlete != null)
					DataManager.Instance.Athletes.AddOrUpdate(athlete);

				return athlete;
			});
		}

		public Task<Athlete> GetAthleteById(string id, bool force = false)
		{
			return new Task<Athlete>(() =>
			{
				Athlete a = null;

				if(!force)
					DataManager.Instance.Athletes.TryGetValue(id, out a);

				a = a ?? Client.GetTable<Athlete>().LookupAsync(id).Result;

				if(a != null)
				{
					a.IsDirty = false;
					DataManager.Instance.Athletes.AddOrUpdate(a);
				}

				return a;				
			});
		}

		/// <summary>
		/// Loads all the leagues for specific athlete
		/// </summary>
		/// <returns>The all leagues by athlete.</returns>
		/// <param name="athlete">Athlete.</param>
		public Task<List<League>> GetAllLeaguesForAthlete(Athlete athlete)
		{
			return new Task<List<League>>(() =>
			{
				var leagueIds = Client.GetTable<Membership>().Where(m => m.AthleteId == athlete.Id && m.AbandonDate == null).Select(m => m.LeagueId).ToListAsync().Result;

				//Wipe out any existing memberships related to the athlete
				foreach(var m in DataManager.Instance.Memberships.Where(m => m.Value.AthleteId == athlete.Id).ToList())
				{
					Membership mem;
					DataManager.Instance.Memberships.TryRemove(m.Key, out mem);
				}

				if(leagueIds.Count == 0)
					return new List<League>();

				var leagues = Client.GetTable<League>().Where(l => leagueIds.Contains(l.Id) && l.IsEnabled).OrderBy(l => l.Name).ToListAsync().Result;
				EnsureAthletesLoadedForLeagues(leagues);
				leagues.ForEach(CacheLeague);

				return leagues;
			});
		}

		public Task SaveAthlete(Athlete athlete)
		{
			return new Task(() =>
			{
				athlete.UserId = AzureService.Instance.Client.CurrentUser.UserId;

				if(athlete.Id == null)
				{
					Client.GetTable<Athlete>().InsertAsync(athlete).Wait();
				}
				else
				{
					Client.GetTable<Athlete>().UpdateAsync(athlete).Wait();
				}

				DataManager.Instance.Athletes.AddOrUpdate(athlete);
			});
		}

		void EnsureAthletesLoadedForLeagues(List<League> leagues)
		{
			foreach(var l in leagues)
			{
				var task = GetAllAthletesForLeague(l);
				task.Start();
				task.Wait();
			}
		}

		void CacheLeague(League l)
		{
			{
				var toRemove = DataManager.Instance.Memberships.Values.Where(m => m.LeagueId == l.Id && !l.Memberships.Select(mm => mm.Id).Contains(m.Id));

				foreach(var m in toRemove)
				{
					Membership mem;
					DataManager.Instance.Memberships.TryRemove(m.Id, out mem);
				}

				foreach(var m in l.Memberships)
				{
					l.MembershipIds.Add(m.Id);
					DataManager.Instance.Memberships.AddOrUpdate(m); //need to update too
					m.Athlete?.LocalRefresh();
				}
			}

			{
				var toRemove = DataManager.Instance.Challenges.Values.Where(c => c.LeagueId == l.Id && !l.OngoingChallenges.Select(cc => cc.Id).Contains(c.Id));

				foreach(var c in toRemove)
				{
					Challenge ch;
					DataManager.Instance.Challenges.TryRemove(c.Id, out ch);
				}

				foreach(var c in l.OngoingChallenges)
				{
					DataManager.Instance.Challenges.AddOrUpdate(c);
				}
			}

			DataManager.Instance.Leagues.AddOrUpdate(l);
			l.RefreshChallenges();
			l.RefreshMemberships();
		}

		public Task DeleteAthlete(string id)
		{
			return new Task(() =>
			{
				Athlete a;
				try
				{
					Client.GetTable<Athlete>().DeleteAsync(new Athlete {
						Id = id
					}).Wait();

					DataManager.Instance.Athletes.TryRemove(id, out a);

					var task = AzureService.Instance.UnregisterAthleteForPush(a);
					task.Start();
					task.Wait();
				}
				catch(HttpRequestException hre)
				{
					if(hre.Message.ContainsNoCase("not found"))
					{
						DataManager.Instance.Athletes.TryRemove(id, out a);
					}
				}
			});
		}

		#endregion

		#region Membership

		public void LoadAthletesForChallenges(List<Challenge> list, bool forceRefresh = false)
		{
			var loaded = list.Where(c => c.Opponent(App.CurrentAthlete.Id) != null).ToList();

			if(forceRefresh)
				loaded.Clear();

			List<string> notLoaded = list.Except(loaded).Select(c => c.ChallengeeAthleteId == App.CurrentAthlete.Id ? c.ChallengerAthleteId : c.ChallengeeAthleteId).ToList();
			notLoaded = notLoaded ?? new List<string>();

			List<Athlete> athletes = new List<Athlete>();
			if(notLoaded.Count > 0)
				athletes = Client.GetTable<Athlete>().Where(a => notLoaded.Contains(a.Id)).ToListAsync().Result;

			athletes.ForEach(DataManager.Instance.Athletes.AddOrUpdate);
		}

		public Task<Membership> GetMembershipById(string id, bool force = false)
		{
			return new Task<Membership>(() =>
			{
				Membership a = null;

				if(force)
					DataManager.Instance.Memberships.TryGetValue(id, out a);

				return a ?? Client.GetTable<Membership>().LookupAsync(id).Result;
			});
		}

		public Task<DateTime?> StartLeague(string id)
		{
			return new Task<DateTime?>(() =>
			{
				var qs = new Dictionary<string, string>();
				qs.Add("id", id);
				var dateTime = Client.InvokeApiAsync("startLeague", null, HttpMethod.Post, qs).Result;
				return (DateTime)dateTime.Root;
			});
		}

		public Task SaveMembership(Membership membership)
		{
			return new Task(() =>
			{
				if(membership.Id == null)
				{
					Client.GetTable<Membership>().InsertAsync(membership).Wait();
					membership.DateCreated = DateTime.UtcNow;
				}
				else
				{
					Client.GetTable<Membership>().UpdateAsync(membership).Wait();
				}

				DataManager.Instance.Memberships.AddOrUpdate(membership);
				membership.LocalRefresh();
			});
		}

		public Task DeleteMembership(string id)
		{
			return new Task(() =>
			{
				Membership m;
				try
				{
					DataManager.Instance.Memberships.TryRemove(id, out m);
					var challenges = DataManager.Instance.Challenges.Values.Where(c => c.LeagueId == m.LeagueId && c.InvolvesAthlete(m.AthleteId)).ToList();

					Challenge ch;
					challenges.ForEach(c => DataManager.Instance.Challenges.TryRemove(c.Id, out ch));

					Client.GetTable<Membership>().DeleteAsync(new Membership {
						Id = id
					}).Wait();

					m.LocalRefresh();
					var task = AzureService.Instance.UpdateAthleteNotificationHubRegistration(m.Athlete);
					task.Start();
					task.Wait();
				}
				catch(HttpRequestException hre)
				{
					if(hre.Message.ContainsNoCase("not found"))
					{
						DataManager.Instance.Memberships.TryRemove(id, out m);
					}
				}
				catch(Exception e)
				{
					Debug.WriteLine(e);
				}
			});			
		}

		#endregion

		#region Challenge

		public Task SaveChallenge(Challenge challenge)
		{
			return new Task(() =>
			{
				if(challenge.Id == null)
				{
					Client.GetTable<Challenge>().InsertAsync(challenge).Wait();
				}
				else
				{
					Client.GetTable<Challenge>().UpdateAsync(challenge).Wait();
				}

				DataManager.Instance.Challenges.AddOrUpdate(challenge);
				challenge.League.RefreshChallenges();
			});
		}

		public Task DeclineChallenge(string id)
		{
			return new Task(() =>
			{
				Challenge m;
				try
				{
					var qs = new Dictionary<string, string> { {
							"id",
							id
						}
					};

					var result = Client.InvokeApiAsync("declineChallenge", HttpMethod.Get, qs).Result;
					DataManager.Instance.Challenges.TryRemove(id, out m);
				}
				catch(HttpRequestException hre)
				{
					if(hre.Message.ContainsNoCase("not found"))
					{
						DataManager.Instance.Challenges.TryRemove(id, out m);
					}
				}
				catch(Exception e)
				{
					Debug.WriteLine(e);
				}
			});
		}

		public Task RevokeChallenge(string id)
		{
			return new Task(() =>
			{
				Challenge m;
				try
				{
					var qs = new Dictionary<string, string> { {
							"id",
							id
						}
					};
					var result = Client.InvokeApiAsync("revokeChallenge", HttpMethod.Get, qs).Result;
					DataManager.Instance.Challenges.TryRemove(id, out m);
				}
				catch(HttpRequestException hre)
				{
					if(hre.Message.ContainsNoCase("not found"))
					{
						DataManager.Instance.Challenges.TryRemove(id, out m);
					}
				}
				catch(Exception e)
				{
					Debug.WriteLine(e);
				}
			});
		}

		public Task<List<Challenge>> GetChallengesForMembership(Membership membership)
		{
			return new Task<List<Challenge>>(() =>
			{
				var qs = new Dictionary<string, string>();
				qs.Add("membershipId", membership.Id);
				qs.Add("$expand", "MatchResult");
				var challenges = Client.InvokeApiAsync<string, List<Challenge>>("getChallengesForMembership", null, HttpMethod.Get, qs).Result;
				if(challenges != null)
				{
					var cached = DataManager.Instance.Challenges.Values.Where(c => c.InvolvesAthlete(membership.Athlete.Id) && c.IsCompleted).ToList();
					var stale = cached.Except(challenges).ToList();

					foreach(var c in stale)
					{
						Challenge ch;
						DataManager.Instance.Challenges.TryRemove(c.Id, out ch);
					}

					challenges.ForEach(c => DataManager.Instance.Challenges.AddOrUpdate(c));

					LoadAthletesForChallenges(challenges);
					return challenges;
				}
				return new List<Challenge>();
			});
		}

		public Task PostMatchResults(Challenge challenge)
		{
			return new Task(() =>
			{
				var completedChallenge = Client.InvokeApiAsync<List<GameResult>, Challenge>("postMatchResults", challenge.MatchResult).Result;
				if(completedChallenge != null)
				{
					challenge.DateCompleted = completedChallenge.DateCompleted;
					challenge.MatchResult = new List<GameResult>();
					completedChallenge.MatchResult.ForEach(challenge.MatchResult.Add);
				}
			});
		}

		public Task AcceptChallenge(Challenge challenge)
		{
			return new Task(() =>
			{
				var qs = new Dictionary<string, string>();
				qs.Add("id", challenge.Id);
				var token = Client.InvokeApiAsync("acceptChallenge", null, HttpMethod.Post, qs).Result;
				var acceptedChallenge = JsonConvert.DeserializeObject<Challenge>(token.ToString());
				if(acceptedChallenge != null)
				{
					challenge.DateAccepted = acceptedChallenge.DateAccepted;
				}
			});
		}

		public Task<Challenge> GetChallengeById(string id, bool force = false)
		{
			return new Task<Challenge>(() =>
			{
				Challenge a = null;

				if(!force)
					DataManager.Instance.Challenges.TryGetValue(id, out a);

				a = a ?? Client.GetTable<Challenge>().LookupAsync(id).Result;
				DataManager.Instance.Challenges.AddOrUpdate(a);
				return a;
			});
		}

		public Task NagAthlete(string challengeId)
		{
			return new Task(() =>
			{
				var qs = new Dictionary<string, string>();
				qs.Add("challengeId", challengeId);
				var g = Client.InvokeApiAsync("nagAthlete", null, HttpMethod.Get, qs).Result;
			});
		}

		#endregion
	}

	#region ChallengeExpandHandler

	/// <summary>
	/// This class is needed to pull down properties that are complex objects - Azure omits complex/Navigation properties by default
	/// You need to 'expand' the property in order for it to be included
	/// </summary>
	public class ChallengeExpandHandler : DelegatingHandler
	{
		protected override async Task<HttpResponseMessage>
		SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			bool requestToTodoTable = request.RequestUri.PathAndQuery
				.StartsWith("/tables/Challenge", StringComparison.OrdinalIgnoreCase)
			                          && request.Method == HttpMethod.Get;
			if(requestToTodoTable)
			{
				UriBuilder builder = new UriBuilder(request.RequestUri);
				string query = builder.Query;
				if(!query.Contains("$expand"))
				{
					if(string.IsNullOrEmpty(query))
					{
						query = string.Empty;
					}
					else
					{
						query = query + "&";
					}

					query = query + "$expand=MatchResult";
					builder.Query = query.TrimStart('?');
					request.RequestUri = builder.Uri;
				}
			}

			var result = await base.SendAsync(request, cancellationToken);
			return result;
		}
	}

	#endregion

	#region LeagueExpandHandler

	/// <summary>
	/// This class is needed to pull down properties that are complex objects - Azure omits complex/Navigation properties by default
	/// You need to 'expand' the property in order for it to be included
	/// </summary>
	public class LeagueExpandHandler : DelegatingHandler
	{
		protected override async Task<HttpResponseMessage>
		SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			bool requestToTodoTable = request.RequestUri.PathAndQuery
				.StartsWith("/tables/League", StringComparison.OrdinalIgnoreCase)
			                          && request.Method == HttpMethod.Get;
			if(requestToTodoTable)
			{
				UriBuilder builder = new UriBuilder(request.RequestUri);
				string query = builder.Query;
				if(!query.Contains("$expand"))
				{
					if(string.IsNullOrEmpty(query))
					{
						query = string.Empty;
					}
					else
					{
						query = query + "&";
					}

					query = query + "$expand=Memberships,OngoingChallenges";
					builder.Query = query.TrimStart('?');
					request.RequestUri = builder.Uri;
				}
			}

			var result = await base.SendAsync(request, cancellationToken);
			return result;
		}
	}

	#endregion
}