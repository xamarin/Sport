using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sport.Mobile.Shared
{
	public class LeagueManager : BaseManager<League>
	{
		public override string Identifier => "League";

		public Task<DateTime?> StartLeague(string id)
		{
			return new Task<DateTime?>(() => {
				var qs = new Dictionary<string, string>();
				qs.Add("id", id);
				var dateTime = AzureService.Instance.Client.InvokeApiAsync("startLeague", null, HttpMethod.Post, qs).Result;
				return (DateTime)dateTime.Root;
			});
		}

		async public override Task<League> GetItemAsync(string id, bool forceRefresh = false)
		{
			if(forceRefresh)
			{
				await AzureService.Instance.GameResultManager.PullLatestAsync().ConfigureAwait(false);
				await AzureService.Instance.ChallengeManager.PullLatestAsync().ConfigureAwait(false);
				await AzureService.Instance.MembershipManager.PullLatestAsync().ConfigureAwait(false);
			}

			return await base.GetItemAsync(id, forceRefresh);
		}
	}
}