using Microsoft.Azure.Mobile.Server;
using System;

namespace Sport
{
	public class ChallengeDto : ChallengeBase { }

    public class ChallengeBase : EntityData
    {
        public string LeagueId
        {
            get;
            set;
        }

        public string ChallengerAthleteId
        {
            get;
            set;
        }

        public string ChallengeeAthleteId
        {
            get;
            set;
        }

        public string CustomMessage
        {
            get;
            set;
        }

		public int? BattleForRank
		{
			get;
			set;
		}

		public DateTimeOffset? ProposedTime
		{
			get;
			set;
		}

		public DateTimeOffset? DateAccepted
		{
			get;
			set;
		}

		public DateTimeOffset? DateCompleted
		{
			get;
			set;
		}
	}
}