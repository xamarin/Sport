using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sport.Mobile.Shared
{
	public class GameResult : BaseModel
	{
		Challenge _league;
		[JsonIgnore]
		public Challenge League
		{
			get
			{
				if(_league == null)
				{
					Task.Run(async () => {
						_league = await AzureService.Instance.ChallengeManager.Table.LookupAsync(ChallengeId);
					}).Wait();
				}

				return _league;
			}
		}

		string _challengeId;

		public string ChallengeId
		{
			get
			{
				return _challengeId;
			}
			set
			{
				SetPropertyChanged(ref _challengeId, value);
			}
		}

		int? challengerScore;

		public int? ChallengerScore
		{
			get
			{
				return challengerScore;
			}
			set
			{
				SetPropertyChanged(ref challengerScore, value);
			}
		}

		int? challengeeScore;

		public int? ChallengeeScore
		{
			get
			{
				return challengeeScore;
			}
			set
			{
				SetPropertyChanged(ref challengeeScore, value);
			}
		}

		int? index;

		public int? Index
		{
			get
			{
				return index;
			}
			set
			{
				SetPropertyChanged(ref index, value);
			}
		}
	}
}