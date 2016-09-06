using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sport
{
	public class Athlete : AthleteBase
	{
		public Athlete()
		{
			Memberships = new HashSet<Membership>();
		}

		[JsonIgnore]
		public ICollection<Membership> Memberships
		{
			get;
			set;
		}
	}
}