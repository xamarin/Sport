using Microsoft.Azure.Mobile.Server;
using System;

namespace Sport
{
	public class LeagueDto : LeagueBase { }

	public class LeagueBase : EntityData
	{
		public string Name
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		public string RulesUrl
		{
			get;
			set;
		}

		public int Season
		{
			get;
			set;
		}

		public int MaxChallengeRange
		{
			get;
			set;
		}

		public int MinHoursBetweenChallenge
		{
			get;
			set;
		}

		public int MatchGameCount
		{
			get;
			set;
		}

		public string Sport
		{
			get;
			set;
		}

		public bool IsAcceptingMembers
		{
			get;
			set;
		}

		public bool IsEnabled
		{
			get;
			set;
		}

		public DateTimeOffset? StartDate
		{
			get;
			set;
		}

		public DateTimeOffset? EndDate
		{
			get;
			set;
		}

		public bool HasStarted
		{
			get;
			set;
		}

		public string ImageUrl
		{
			get;
			set;
		}

		public string CreatedByAthleteId
		{
			get;
			set;
		}
	}
}