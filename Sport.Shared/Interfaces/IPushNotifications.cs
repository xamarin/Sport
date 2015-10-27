using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Sport.Shared
{
	public interface IPushNotifications
	{
		//event EventHandler<IncomingPushNotificationEventArgs> OnPushNotificationReceived;
		void RegisterForPushNotifications();
	}

	public class IncomingPushNotificationEventArgs : EventArgs
	{
		public Dictionary<string, object> Payload
		{
			get;
			set;
		}
	}
}