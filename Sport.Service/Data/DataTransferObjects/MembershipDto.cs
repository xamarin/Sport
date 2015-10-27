using Microsoft.WindowsAzure.Mobile.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Sport
{
	public partial class MembershipDto : MembershipBase
	{
		public DateTimeOffset? DateCreated
		{
			get;
			set;
		}

		//public List<ChallengeDto> OngoingChallenges
		//{
		//	get;
		//	set;
		//}
	}

	public class Test
	{
		public string Name
		{
			get;
			set;
		}
	}
}