using Microsoft.WindowsAzure.Mobile.Service;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sport
{
	public partial class Athlete : AthleteBase
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