using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sport.Mobile.Shared
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
				if(_theme == null)
					_theme = App.Instance.Theming.GetTheme(this);
				
				return _theme;
			}
		}

		Athlete _createdByAthlete;
		[JsonIgnore]
		public Athlete CreatedByAthlete
		{
			get
			{
				if(_createdByAthlete == null)
				{
					Task.Run(async () => {
						_createdByAthlete = await AzureService.Instance.AthleteManager.Table.LookupAsync(CreatedByAthleteId);
					}).Wait();
				}

				return _createdByAthlete;
			}
		}

		List<Membership> _memberships;

		[JsonIgnore]
		public List<Membership> Memberships
		{
			get
			{
				if(_memberships == null)
				{
					Task.Run(async () => {
						_memberships = await AzureService.Instance.MembershipManager.Table.Where(i => i.LeagueId == Id && i.AbandonDate == null).ToListAsync();
					}).Wait();

					Sort();
				}

				return _memberships;
			}
		}

		List<Challenge> _ongoingChallenges;

		[JsonIgnore]
		public List<Challenge> OngoingChallenges
		{
			get
			{
				if(_ongoingChallenges == null)
				{
					Task.Run(async () => {
						_ongoingChallenges = await AzureService.Instance.ChallengeManager.Table.Where(i => i.LeagueId == Id && i.DateCompleted == null).ToListAsync();
					}).Wait();
				}

				return _ongoingChallenges;
			}
		}

		List<Challenge> _pastChallenges;

		[JsonIgnore]
		public List<Challenge> PastChallenges
		{
			get
			{
				if(_pastChallenges == null)
				{
					Task.Run(async () => {
						_pastChallenges = await AzureService.Instance.ChallengeManager.Table.Where(i => i.LeagueId == Id && i.DateCompleted != null).ToListAsync();
					}).Wait();
				}

				return _pastChallenges;
			}
		}

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

		//int _maxChallengeRange;

		//public int MaxChallengeRange
		//{
		//	get
		//	{
		//		return _maxChallengeRange;
		//	}
		//	set
		//	{
		//		SetPropertyChanged(ref _maxChallengeRange, value);
		//		SetPropertyChanged("LeagueDetails");
		//	}
		//}

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
				return string.Format("There {2} {0} member{1} enjoying sport", Memberships.Count, Memberships.Count == 1 ? "" : "s", Memberships.Count == 1 ? "is" : "are");
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

				if(m.Athlete != null)
					return $"{m.Athlete.Alias} has been ranked {m.RankDescription}";

				return null;
			}
		}

		public string LeagueDetails
		{
			get
			{
				var sb = new StringBuilder();
				sb.AppendLine(string.Format("• there {2} {0} game{1} to a match", MatchGameCount, MatchGameCount == 1 ? "" : "s", MatchGameCount == 1 ? "is" : "are"));
				sb.AppendLine(string.Format("• you must wait {0} hour{1} before challenging after a loss", MinHoursBetweenChallenge, MinHoursBetweenChallenge == 1 ? "" : "s"));
				sb.AppendLine("• declining a challenge 3 times results in a forfeit of rank");

				return sb.ToString();
			}
		}

		void Initialize()
		{
			StartDate = DateTime.Now.AddDays(7);
			EndDate = DateTime.Now.AddMonths(6);
			//Memberships = new List<Membership>();

			MinHoursBetweenChallenge = 48;
			MatchGameCount = 3;

			IsAcceptingMembers = true;
			IsEnabled = true;
		}

		public override void LocalRefresh()
		{
			base.LocalRefresh();

			_memberships = null;
			_createdByAthlete = null;
			_ongoingChallenges = null;
			_pastChallenges = null;
		}

		public override bool Equals(object obj)
		{
			var comp = new LeagueComparer();
			return comp.Equals(this, obj as League);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public void Sort()
		{
			_memberships.Sort(new MembershipSortComparer());
			for(int i = 0; i < _memberships.Count(); i++)
				_memberships[i].CurrentRank = i;
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
						   && x.OngoingChallenges?.Count == y.OngoingChallenges?.Count;
			              //&& x.MembershipIds?.Count == y.MembershipIds?.Count;

			return isEqual;
		}

		public int GetHashCode(League obj)
		{
			return obj.Id != null ? obj.Id.GetHashCode() : base.GetHashCode();
		}
	}

	#endregion
}