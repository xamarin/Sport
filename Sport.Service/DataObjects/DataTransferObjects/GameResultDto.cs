using Newtonsoft.Json;
using Microsoft.Azure.Mobile.Server;

namespace Sport
{
	public class GameResultDto : GameResultBase { }

	public class GameResultBase : EntityData
	{
		public string ChallengeId
		{
			get;
			set;
		}

		public int Index
		{
			get;
			set;
		}

		public int? ChallengerScore
		{
			get;
			set;
		}

		public int? ChallengeeScore
		{
			get;
			set;
		}
	}
}