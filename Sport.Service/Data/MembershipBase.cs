using Microsoft.WindowsAzure.Mobile.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Sport
{
	public partial class MembershipBase : EntityData
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