using System;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Linq;

namespace Sport.Shared
{
	public class MatchResultFormViewModel : BaseViewModel
	{
		string _challengeId;

		public string ChallengeId
		{
			get
			{
				return _challengeId;
			}
			set
			{
				_challengeId = value;
				SetPropertyChanged("Challenge");
			}
		}

		public Challenge Challenge
		{
			get
			{
				return ChallengeId == null ? null : DataManager.Instance.Challenges.Get(ChallengeId);
			}
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

				var task = AzureService.Instance.PostMatchResults(Challenge);
				await RunSafe(task);

				return !task.IsFaulted;
			}
		}
	}
}