using System;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace Sport.Shared
{
	public class Athlete : BaseModel
	{
		public Athlete()
		{
			Initialize();
		}

		public Athlete(GoogleUserProfile profile)
		{
			Name = profile.Name;
			Email = profile.Email;
			AuthenticationId = profile.Id;
			ProfileImageUrl = profile.Picture;
			Initialize();
		}

		void Initialize()
		{
		}

		public List<string> MembershipIds
		{
			get;
			set;
		}

		string _userId;

		public string UserId
		{
			get
			{
				return _userId;
			}
			set
			{
				SetPropertyChanged(ref _userId, value);
			}
		}

		string _alias;

		public string Alias
		{
			get
			{
				return _alias;
			}
			set
			{
				_alias = value;
			}
		}

		string _name;

		public string Name
		{
			get
			{
				if(App.CurrentAthlete != null && App.CurrentAthlete.Id != Id && !string.IsNullOrEmpty(_name))
					return _name.Split(' ')[0];

				return _name;
			}
			set
			{
				SetPropertyChanged(ref _name, value);
			}
		}

		string _email;

		public string Email
		{
			get
			{
				if(App.CurrentAthlete != null && App.CurrentAthlete.Id != Id)
					return "user@demo.com";

				return _email;
			}
			set
			{
				SetPropertyChanged(ref _email, value);
			}
		}

		string _authenticationId;

		public string AuthenticationId
		{
			get
			{
				return _authenticationId;
			}
			set
			{
				SetPropertyChanged(ref _authenticationId, value);
			}
		}

		bool _isAdmin;

		public bool IsAdmin
		{
			get
			{
				return _isAdmin;
			}
			set
			{
				SetPropertyChanged(ref _isAdmin, value);
			}
		}

		List<Membership> _memberships = new List<Membership>();

		[JsonIgnore]
		public List<Membership> Memberships
		{
			get
			{
				return _memberships;
			}
			set
			{
				SetPropertyChanged(ref _memberships, value);
				SetPropertyChanged("Leagues");
			}
		}

		string _deviceToken;

		public string DeviceToken
		{
			get
			{
				return _deviceToken;
			}
			set
			{
				SetPropertyChanged(ref _deviceToken, value);
			}
		}

		string _devicePlatform;

		public string DevicePlatform
		{
			get
			{
				return _devicePlatform;
			}
			set
			{
				SetPropertyChanged(ref _devicePlatform, value);
			}
		}

		string _notificationRegistrationId;

		public string NotificationRegistrationId
		{
			get
			{
				return _notificationRegistrationId;
			}
			set
			{
				SetPropertyChanged(ref _notificationRegistrationId, value);
			}
		}

		string _profileImageUrl;

		public string ProfileImageUrl
		{
			get
			{
				return _profileImageUrl;
			}
			set
			{
				SetPropertyChanged(ref _profileImageUrl, value);
			}
		}

		public override void LocalRefresh()
		{
			//TODO Error here when deleting an existing league
			_memberships.Clear();
			DataManager.Instance.Memberships.Values.Where(m => m.AthleteId == Id).OrderBy(l => l.League?.Name).ToList().ForEach(_memberships.Add);
		}

		[JsonIgnore]
		public List<League> Leagues
		{
			get
			{
				return Memberships.Select(m => m.League).ToList();
			}
		}

		public override bool Equals(object obj)
		{
			var comp = new AthleteComparer();
			return comp.Equals(this, obj as Athlete);
		}
	}

	#region Comparers

	public class AthleteComparer : IEqualityComparer<Athlete>
	{
		public bool Equals(Athlete x, Athlete y)
		{
			if(x == null || y == null)
				return false;

			var isEqual = x.Id == y.Id
			              && x.Alias == y.Alias
			              && x.Email == y.Email
			              && x.Name == y.Name
			              && x.ProfileImageUrl == y.ProfileImageUrl;

			return isEqual;
		}

		public int GetHashCode(Athlete obj)
		{
			return obj.Id != null ? obj.Id.GetHashCode() : base.GetHashCode();
		}
	}

	#endregion
}