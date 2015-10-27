using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sport.Service
{
	internal class Constants
	{
		internal static readonly string HubConnectionString = "Endpoint=sb://sportchallengematchrank.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=COuEk5dgadpaYranvlOFgz1B103SHaO68lGfoVh+a74=";
		internal static readonly string HubName = "defaultnotificationhub";
	}
}
