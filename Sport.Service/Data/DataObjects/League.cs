using Microsoft.WindowsAzure.Mobile.Service;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sport
{
	public partial class League : LeagueBase
	{
		public League()
		{
			Memberships = new HashSet<Membership>();
			Challenges = new HashSet<Challenge>();
		}

		public virtual Athlete CreatedByAthlete
		{
			get;
			set;
		}

		public virtual ICollection<Membership> Memberships
		{
			get;
			set;
		}

		public virtual ICollection<Challenge> Challenges
		{
			get;
			set;
		}
	}
}