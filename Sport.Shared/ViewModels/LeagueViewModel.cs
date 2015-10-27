using System.Linq;
using System;

namespace Sport.Shared
{
	public class LeagueViewModel : BaseViewModel
	{
		string _leagueId;

		public string LeagueId
		{
			get
			{
				return _leagueId;
			}
			set
			{
				SetPropertyChanged(ref _leagueId, value);

				_league = null;
				SetPropertyChanged("League");
			}
		}

		League _league;

		public League League
		{
			get
			{
				if(_league == null)
					_league = LeagueId == null ? null : DataManager.Instance.Leagues.Get(LeagueId);

				return _league;
			}
		}

		public bool HasChallenge
		{
			get
			{
				return CurrentMembership != null && CurrentMembership.OngoingChallenge != null;
			}
		}

		public Membership CurrentMembership
		{
			get
			{
				return App.CurrentAthlete?.Memberships.FirstOrDefault(m => m.LeagueId == League?.Id);
			}
		}

		public bool IsFirstPlace
		{
			get
			{
				return CurrentMembership != null && CurrentMembership.CurrentRank == 0 && CurrentMembership.League.HasStarted;
			}
		}

		public bool CanGetRules
		{
			get
			{
				return !string.IsNullOrWhiteSpace(League.RulesUrl);	
			}
		}

		protected string _praisePhrase;

		public string PraisePhrase
		{
			get
			{
				if(_praisePhrase == null)
				{
					var random = new Random().Next(0, App.PraisePhrases.Count - 1);
					_praisePhrase = App.PraisePhrases[random];
				}
				return "you're {0}".Fmt(_praisePhrase);
			}
		}

		public bool IsMember
		{
			get
			{
				return CurrentMembership != null;
			}
		}

		public string CreatedBy
		{
			get
			{
				return League == null || League.CreatedByAthlete == null ? null : "created on {0} by {1}".Fmt(League.DateCreated.Value.ToString("MMMM dd, yyyy"), League.CreatedByAthlete.Name);
			}
		}

		public bool HasLeaderOtherThanSelf
		{
			get
			{
				if(League.Memberships.Count == 0)
					return false;

				return LeaderMembership?.AthleteId != App.CurrentAthlete.Id;
			}
		}

		public Membership LeaderMembership
		{
			get
			{
				return League.Memberships.OrderBy(m => m.CurrentRank).FirstOrDefault();
			}
		}

		public bool IsNotMemberAndLeagueStarted
		{
			get
			{
				if(League == null)
					return false;

				return !IsMember && League.HasStarted;
			}
		}

		public bool IsMemberAndLeagueStarted
		{
			get
			{
				if(League == null)
					return false;

				return IsMember && League.HasStarted;
			}
		}

		bool _isLast;

		public bool IsLast
		{
			get
			{
				return _isLast;
			}
			set
			{
				SetPropertyChanged(ref _isLast, value);
			}
		}

		string _emptyMessage;

		public string EmptyMessage
		{
			get
			{
				return _emptyMessage;
			}
			set
			{
				SetPropertyChanged(ref _emptyMessage, value);
			}
		}

		public bool CanChallenge
		{
			get
			{
				return GetBestChallengee != null;
			}
		}

		public Membership GetBestChallengee
		{
			get
			{
				if(!League.HasStarted && CurrentMembership != null)
					return null;

				var gap = League.MaxChallengeRange;
				Membership best = null;
				while(best == null && gap > 0 && CurrentMembership != null)
				{
					best = League.Memberships.SingleOrDefault(m => m.CurrentRank == CurrentMembership.CurrentRank - gap);

					if(best == null)
						return null;

					//Ensure no issues with player
					var conflict = best.GetChallengeConflictReason(CurrentMembership.Athlete);
					if(best != null && conflict != null)
						best = null;

					gap--;
				}

				return best;
			}
		}

		public string DateRange
		{
			get
			{
				if(League == null)
					return null;

				var range = "open season";

				if(League != null && League.StartDate.HasValue)
					range = "beginning {0}".Fmt(League.StartDate.Value.ToString("MMM dd, yyyy"));

				if(League != null && League.EndDate.HasValue)
					range += "- {0}".Fmt(League.EndDate.Value.ToString("MMM dd, yyyy"));

				return range;
			}
		}

		public override void NotifyPropertiesChanged()
		{
			_league = null;

			base.NotifyPropertiesChanged();
			SetPropertyChanged("League");
			SetPropertyChanged("Athlete");
			SetPropertyChanged("HasChallenge");
			SetPropertyChanged("IsMember");
			SetPropertyChanged("IsLast");
			SetPropertyChanged("IsMemberAndLeagueStarted");
			SetPropertyChanged("IsNotMemberAndLeagueStarted");
			SetPropertyChanged("IsFirstPlace");
			SetPropertyChanged("EmptyMessage");
			SetPropertyChanged("DateRange");
			SetPropertyChanged("CreatedBy");
			SetPropertyChanged("LeaderMembership");
			SetPropertyChanged("HasLeaderOtherThanSelf");
			SetPropertyChanged("CurrentMembership");
			SetPropertyChanged("CanChallenge");
			SetPropertyChanged("PraisePhrase");
			SetPropertyChanged("GetBestChallengee");
		}
	}
}