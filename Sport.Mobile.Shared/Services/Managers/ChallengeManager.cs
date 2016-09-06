using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;

namespace Sport.Mobile.Shared
{
	public class ChallengeManager : BaseManager<Challenge>
	{
		public override string Identifier => "Challenge";

		async public override Task<Challenge> GetItemAsync(string id, bool forceRefresh = false)
		{
			if(forceRefresh)
				await AzureService.Instance.GameResultManager.PullLatestAsync().ConfigureAwait(false);
	
			return await base.GetItemAsync(id, forceRefresh);
		}

		public async override Task<IList<Challenge>> GetItemsAsync(bool forceRefresh = false)
		{
			if(forceRefresh)
				await AzureService.Instance.GameResultManager.PullLatestAsync();

			var list = await base.GetItemsAsync(forceRefresh);
			return list;
		}

		public Task PostMatchResults(Challenge challenge)
		{
			return new Task(() => {
				var completedChallenge = AzureService.Instance.Client.InvokeApiAsync<List<GameResult>, Challenge>("postMatchResults", challenge.MatchResult).Result;
				if(completedChallenge != null)
				{
					PullLatestAsync().Wait();
					AzureService.Instance.GameResultManager.PullLatestAsync().Wait();
					AzureService.Instance.MembershipManager.PullLatestAsync().Wait();

					challenge.League?.LocalRefresh();
					challenge.LocalRefresh();
				}
			});
		}

		public Task NudgeAthlete(string challengeId)
		{
			return new Task(() => {
				var qs = new Dictionary<string, string>();
				qs.Add("challengeId", challengeId);
				var g = AzureService.Instance.Client.InvokeApiAsync("nudgeAthlete", null, HttpMethod.Get, qs).Result;
			});
		}
	}
}