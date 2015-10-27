using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;

namespace Sport.Shared
{
	public class League : BaseModel
	{
		public League()
		{
			Initialize();
		}

		#region Properties

		ColorTheme _theme;

		[JsonIgnore]
		public ColorTheme Theme
		{
			get
			{
				return _theme;
			}
			set
			{
				SetPropertyChanged(ref _theme, value);
			}
		}

		[JsonIgnore]
		public Athlete CreatedByAthlete
		{
			get
			{
				return CreatedByAthleteId == null ? null : DataManager.Instance.Athletes.Get(CreatedByAthleteId);
			}
		}

		public List<string> MembershipIds
		{
			get;
			set;
		} = new List<string>();

		public string Index
		{
			get;
			set;
		}

		bool _hasStarted;

		public bool HasStarted
		{
			get
			{
				return _hasStarted;
			}
			set
			{
				SetPropertyChanged(ref _hasStarted, value);
			}
		}

		string _name;

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				SetPropertyChanged(ref _name, value);
			}
		}

		string _rulesUrl;

		public string RulesUrl
		{
			get
			{
				return _rulesUrl;
			}
			set
			{
				SetPropertyChanged(ref _rulesUrl, value);
			}
		}

		string _sport;

		public string Sport
		{
			get
			{
				return _sport;
			}
			set
			{
				SetPropertyChanged(ref _sport, value);
			}
		}

		bool _isEnabled;

		public bool IsEnabled
		{
			get
			{
				return _isEnabled;
			}
			set
			{
				SetPropertyChanged(ref _isEnabled, value);
			}
		}

		bool _isAcceptingMembers;

		public bool IsAcceptingMembers
		{
			get
			{
				return _isAcceptingMembers;
			}
			set
			{
				SetPropertyChanged(ref _isAcceptingMembers, value);
				SetPropertyChanged("LeagueDetails");
			}
		}

		int _maxChallengeRange;

		public int MaxChallengeRange
		{
			get
			{
				return _maxChallengeRange;
			}
			set
			{
				SetPropertyChanged(ref _maxChallengeRange, value);
				SetPropertyChanged("LeagueDetails");
			}
		}

		int _minHoursBetweenChallenge;

		public int MinHoursBetweenChallenge
		{
			get
			{
				return _minHoursBetweenChallenge;
			}
			set
			{
				SetPropertyChanged(ref _minHoursBetweenChallenge, value);
				SetPropertyChanged("LeagueDetails");
			}
		}

		int _matchGameCount;

		public int MatchGameCount
		{
			get
			{
				return _matchGameCount;
			}
			set
			{
				SetPropertyChanged(ref _matchGameCount, value);
				SetPropertyChanged("LeagueDetails");
			}
		}

		string description;

		public string Description
		{
			get
			{
				return description;
			}
			set
			{
				SetPropertyChanged(ref description, value);
			}
		}

		int _season;

		public int Season
		{
			get
			{
				return _season;
			}
			set
			{
				SetPropertyChanged(ref _season, value);
			}
		}

		List<Membership> _memberships = new List<Membership>();

		public List<Membership> Memberships
		{
			get
			{
				return _memberships;
			}
			set
			{
				SetPropertyChanged(ref _memberships, value);
			}
		}

		List<Challenge> _ongoingChallenges = new List<Challenge>();

		public List<Challenge> OngoingChallenges
		{
			get
			{
				return _ongoingChallenges;
			}
			set
			{
				SetPropertyChanged(ref _ongoingChallenges, value);
			}
		}

		List<Challenge> _pastChallenges = new List<Challenge>();

		public List<Challenge> PastChallenges
		{
			get
			{
				return _pastChallenges;
			}
			set
			{
				SetPropertyChanged(ref _pastChallenges, value);
			}
		}

		string imageUrl;

		public string ImageUrl
		{
			get
			{
				return imageUrl;
			}
			set
			{
				SetPropertyChanged(ref imageUrl, value);
			}
		}

		string createdByAthleteId;

		public string CreatedByAthleteId
		{
			get
			{
				return createdByAthleteId;
			}
			set
			{
				SetPropertyChanged(ref createdByAthleteId, value);
				SetPropertyChanged("CreatedByAthlete");
			}
		}

		DateTime? _startDate;

		public DateTime? StartDate
		{
			get
			{
				return _startDate;
			}
			set
			{
				SetPropertyChanged(ref _startDate, value);
				SetPropertyChanged("DateRange");
				SetPropertyChanged("LeagueDetails");
			}
		}

		DateTime? _endDate;

		public DateTime? EndDate
		{
			get
			{
				return _endDate;
			}
			set
			{
				SetPropertyChanged(ref _endDate, value);
				SetPropertyChanged("DateRange");
				SetPropertyChanged("LeagueDetails");
			}
		}

		#endregion

		public string MemberCount
		{
			get
			{
				return "There {2} {0} member{1} enjoying sport".Fmt(Memberships.Count, Memberships.Count == 1 ? "" : "s", Memberships.Count == 1 ? "is" : "are");
			}
		}

		public string LeaderRankDescription
		{
			get
			{
				if(!HasStarted)
					return "This league hasn't started yet";

				if(Memberships?.Count == 0)
					return "The league has no members - you should totally join!";

				var m = Memberships?.First();
				return "{0} has been ranked {1}".Fmt(m.Athlete.Alias, m.RankDescription);
			}
		}

		public string LeagueDetails
		{
			get
			{
				var sb = new StringBuilder();
				sb.AppendLine("• you may challenge up to {0} spot above your current rank".Fmt(MaxChallengeRange));
				sb.AppendLine("• there {2} {0} game{1} to a match".Fmt(MatchGameCount, MatchGameCount == 1 ? "" : "s", MatchGameCount == 1 ? "is" : "are"));
				sb.AppendLine("• you must wait {0} hour{1} before challenging after a loss".Fmt(MinHoursBetweenChallenge, MinHoursBetweenChallenge == 1 ? "" : "s"));
				sb.AppendLine("• declining a challenge 3 times results in a forfeit of rank");

				return sb.ToString();
			}
		}

		void Initialize()
		{
			StartDate = DateTime.Now.AddDays(7);
			EndDate = DateTime.Now.AddMonths(6);
			Memberships = new List<Membership>();

			MaxChallengeRange = 1;
			MinHoursBetweenChallenge = 48;
			MatchGameCount = 3;

			IsAcceptingMembers = true;
			IsEnabled = true;
		}

		public void RefreshMemberships()
		{
			_memberships.Clear();

			DataManager.Instance.Memberships.Values.Where(m => m.LeagueId == Id).OrderBy(m => m.CurrentRank).ToList().ForEach(_memberships.Add);
			SetPropertyChanged("Memberships");

			_memberships.ForEach(m => m.LocalRefresh(false));
		}

		public void RefreshChallenges()
		{
			if(_ongoingChallenges != null)
			{
				_ongoingChallenges.Clear();
				DataManager.Instance.Challenges.Values.Where(c => c.LeagueId == Id && !c.IsCompleted)
					.OrderByDescending(c => c.ProposedTime).ToList().ForEach(_ongoingChallenges.Add);
				
				SetPropertyChanged("OngoingChallenges");
			}

			if(_pastChallenges != null)
			{
				_pastChallenges.Clear();
				DataManager.Instance.Challenges.Values.Where(c => c.LeagueId == Id && c.IsCompleted)
					.OrderByDescending(c => c.ProposedTime).ToList().ForEach(_pastChallenges.Add);

				SetPropertyChanged("PastChallenges");
			}
		}

		public override void LocalRefresh()
		{
			base.LocalRefresh();
			RefreshMemberships();
			RefreshChallenges();
		}

		public override bool Equals(object obj)
		{
			var comp = new LeagueComparer();
			return comp.Equals(this, obj as League);
		}
	}

	#region Comparers

	public class LeagueIdComparer : IEqualityComparer<League>
	{
		public bool Equals(League x, League y)
		{
			if(x == null || y == null)
				return false;
			
			return x.Id == y.Id;
		}

		public int GetHashCode(League obj)
		{
			return obj.Id != null ? obj.Id.GetHashCode() : base.GetHashCode();
		}
	}

	public class LeagueComparer : IEqualityComparer<League>
	{
		public bool Equals(League x, League y)
		{
			if(x == null || y == null)
				return false;

			var isEqual = x.Id == y.Id
			              && x.UpdatedAt == y.UpdatedAt
			              && x.Name == y.Name
			              && x.Description == y.Description
			              && x.EndDate == y.EndDate
			              && x.StartDate == y.StartDate
			              && x.HasStarted == y.HasStarted
			              && x.ImageUrl == y.ImageUrl
			              && x.IsEnabled == y.IsEnabled
			              && x.IsAcceptingMembers == y.IsAcceptingMembers
			              && x.MatchGameCount == y.MatchGameCount
			              && x.RulesUrl == y.RulesUrl
			              && x.OngoingChallenges?.Count == y.OngoingChallenges?.Count
			              && x.MembershipIds?.Count == y.MembershipIds?.Count;

			return isEqual;
		}

		public int GetHashCode(League obj)
		{
			return obj.Id != null ? obj.Id.GetHashCode() : base.GetHashCode();
		}
	}

	public class LeagueSortComparer : IComparer<LeagueViewModel>
	{
		public int Compare(LeagueViewModel x, LeagueViewModel y)
		{
			if(x?.League == null || y?.League == null)
				return -1;
			
			return string.Compare(x.League?.Name, y.League?.Name, System.StringComparison.OrdinalIgnoreCase);
		}
	}

	#endregion
}