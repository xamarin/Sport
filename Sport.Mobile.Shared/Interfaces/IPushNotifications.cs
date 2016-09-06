using System;
using System.Collections.Generic;

namespace Sport.Mobile.Shared
{
	public interface IPushNotifications
	{
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