using Microsoft.WindowsAzure.Mobile.Service;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sport
{
	public partial class GameResult : GameResultBase
	{
		public Challenge Challenge
		{
			get;
			set;
		}
	}
}