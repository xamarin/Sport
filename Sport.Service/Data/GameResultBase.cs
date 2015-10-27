using Microsoft.WindowsAzure.Mobile.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Sport
{
	public partial class GameResultBase : EntityData
	{
		public string ChallengeId
		{
			get;
			set;
		}

		public int Index
		{
			get;
			set;
		}

		public int? ChallengerScore
		{
			get;
			set;
		}

		public int? ChallengeeScore
		{
			get;
			set;
		}
	}
}