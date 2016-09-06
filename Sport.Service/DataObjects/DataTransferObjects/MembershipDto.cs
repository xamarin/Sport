using Microsoft.Azure.Mobile.Server;
using Newtonsoft.Json;
using System;

namespace Sport
{
	public class MembershipDto : MembershipBase { }

	public class MembershipBase : EntityData
	{
		public string AthleteId
		{
			get;
			set;
		}

		public string LeagueId
		{
			get;
			set;
		}

		public int CurrentRank
		{
			get;
			set;
		}

		public DateTimeOffset? AbandonDate
		{
			get;
			set;
		}

		public bool IsAdmin
		{
			get;
			set;
		}

		public DateTimeOffset? LastRankChange
		{
			get;
			set;
		}
	}
}