using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sport
{
	public class League : LeagueBase
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

        [JsonIgnore]
        public virtual ICollection<Membership> Memberships
		{
			get;
			set;
		}

        [JsonIgnore]
        public virtual ICollection<Challenge> Challenges
		{
			get;
			set;
		}
	}
}