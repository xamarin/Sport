using Microsoft.WindowsAzure.Mobile.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Sport
{
	public partial class ChallengeDto : ChallengeBase
	{
		public ChallengeDto() : base()
		{
			MatchResult = new List<GameResultDto>();
		}

		public DateTimeOffset? DateCreated
		{
			get;
			set;
		}

		public List<GameResultDto> MatchResult
		{
			get;
			set;
		}
	}
}