using System;

namespace Sport.Mobile.Shared
{
	public interface IUserProfile
	{
		string Id
		{
			get;
			set;
		}

		string Name
		{
			get;
			set;
		}

		string Email
		{
			get;
			set;
		}

		string PhotoUrl
		{
			get;
			set;
		}
	}
}
