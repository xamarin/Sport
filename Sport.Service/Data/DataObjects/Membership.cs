using Microsoft.WindowsAzure.Mobile.Service;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Mobile.Service.Tables;
using Newtonsoft.Json;

namespace Sport
{
	public partial class Membership : MembershipBase
	{
		public virtual League League
		{
			get;
			set;
		}

		public virtual Athlete Athlete
		{
			get;
			set;
		}
	}
}