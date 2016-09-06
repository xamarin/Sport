using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sport
{
	public class Challenge : ChallengeBase
	{
		public Challenge() : base()
		{
			MatchResult = new List<GameResult>();
		}

		[JsonIgnore]
		public virtual League League
		{
			get;
			set;
		}

		[JsonIgnore]
		public virtual Athlete ChallengerAthlete
		{
			get;
			set;
		}

		[JsonIgnore]
		public virtual Athlete ChallengeeAthlete
		{
			get;
			set;
		}

		public List<GameResult> MatchResult
		{
			get;
			set;
		}
	}
}