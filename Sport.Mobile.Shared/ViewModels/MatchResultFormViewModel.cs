using System;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Linq;

namespace Sport.Mobile.Shared
{
	public class MatchResultFormViewModel : BaseViewModel
	{
		public Challenge Challenge
		{
			get;
			set;
		}

		async public Task<bool> PostMatchResults()
		{
			using(new Busy(this))
			{
				foreach(var gr in Challenge.MatchResult.ToList())
				{
					if(gr.ChallengeeScore == null || gr.ChallengerScore == null)
						Challenge.MatchResult.Remove(gr);
				}

				var task = AzureService.Instance.ChallengeManager.PostMatchResults(Challenge);
				await RunSafe(task);

				return !task.IsFaulted;
			}
		}
	}
}