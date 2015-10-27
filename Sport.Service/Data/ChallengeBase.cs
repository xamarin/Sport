using Microsoft.WindowsAzure.Mobile.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Sport
{
    public partial class ChallengeBase : EntityData
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