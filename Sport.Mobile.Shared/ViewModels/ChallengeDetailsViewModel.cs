using Xamarin.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Sport.Mobile.Shared
{
	public class ChallengeDetailsViewModel : ChallengeViewModel
	{
		async public Task<bool> AcceptChallenge()
		{
			Challenge.DateAccepted = DateTime.UtcNow;
			var success = await AzureService.Instance.ChallengeManager.UpdateAsync(Challenge);

			if(success)
			{
				Challenge.League.LocalRefresh();
				NotifyPropertiesChanged();

				MessagingCenter.Send(App.Instance, Messages.ChallengesUpdated);
			}

			return success;
		}

		async public Task<bool> DeclineChallenge()
		{
			await AzureService.Instance.ChallengeManager.RemoveAsync(Challenge);

			Challenge.League.LocalRefresh();
			MessagingCenter.Send(App.Instance, Messages.ChallengesUpdated);
			return true;
		}

		async public Task NudgeAthlete()
		{
			using(new Busy(this))
			{
				var task = AzureService.Instance.ChallengeManager.NudgeAthlete(Challenge.Id);
				await RunSafe(task);

				if(task.IsFaulted)
					return;
			}
		}

		async public Task<bool> RefreshChallenge(bool force = false)
		{
			if(Challenge == null)
				return false;
			
			using(new Busy(this))
			{
				var wasCompleted = Challenge.IsCompleted;
				Challenge = await AzureService.Instance.ChallengeManager.GetItemAsync(Challenge.Id, force);

				if(Challenge == null)
					return false;

				if(!wasCompleted && Challenge.IsCompleted)
				{
					await AzureService.Instance.GameResultManager.PullLatestAsync();
					await AzureService.Instance.MembershipManager.PullLatestAsync();
				}

				NotifyPropertiesChanged();
				return true;
			}
		}

		public override void NotifyPropertiesChanged([System.Runtime.CompilerServices.CallerMemberName] string caller = "")
		{
			Challenge.LocalRefresh();
			Challenge.League?.LocalRefresh();
			base.NotifyPropertiesChanged();
		}
	}
}