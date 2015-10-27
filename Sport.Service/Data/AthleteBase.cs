using Microsoft.WindowsAzure.Mobile.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Sport
{
	public partial class AthleteBase : EntityData
	{
		public string Name
		{
			get;
			set;
		}

		public string AuthenticationId
		{
			get;
			set;
		}

		public string Email
		{
			get;
			set;
		}

		public string Alias
		{
			get;
			set;
		}

		public bool IsAdmin
		{
			get;
			set;
		}

		public string DeviceToken
		{
			get;
			set;
		}

		public string DevicePlatform
		{
			get;
			set;
		}

		public string NotificationRegistrationId
		{
			get;
			set;
		}

		public string ProfileImageUrl
		{
			get;
			set;
		}

		public string UserId
		{
			get;
			set;
		}
	}
}