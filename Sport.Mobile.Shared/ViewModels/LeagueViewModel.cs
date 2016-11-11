using System.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sport.Mobile.Shared
{
	public class LeagueViewModel : BaseViewModel
	{
		public virtual League League
		{
			get;
			set;
		}

		public string LeagueId
		{
			get
			{
				return League?.Id;
			}
		}

		public bool HasChallenge
		{
			get
			{
				return CurrentMembership?.OngoingChallenges?.Count() > 0;
			}
		}

		public Membership CurrentMembership
		{
			get
			{
				return League?.Memberships.FirstOrDefault(m => m.AthleteId == App.Instance.CurrentAthlete?.Id);
			}
		}

		public bool IsFirstPlace
		{
			get
			{
				if(League == null)
					return false;
				
				return CurrentMembership != null && CurrentMembership.CurrentRank == 0 && CurrentMembership.League.HasStarted;
			}
		}

		public bool CanGetRules
		{
			get
			{
				return !string.IsNullOrWhiteSpace(League?.RulesUrl);	
			}
		}

		protected string _praisePhrase;

		public string PraisePhrase
		{
			get
			{
				if(_praisePhrase == null)
				{
					var random = new Random().Next(0, App.Instance.PraisePhrases.Count - 1);
					_praisePhrase = App.Instance.PraisePhrases[random];
				}
				return $"you're {_praisePhrase}";
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
				return League == null || League?.CreatedByAthlete == null ? null : $"created on {League?.CreatedAt.Value.ToString("MMMM dd, yyyy")} by {League?.CreatedByAthlete.Name}";
			}
		}

		public bool HasLeaderOtherThanSelf
		{
			get
			{
				if(League?.Memberships.Count == 0)
					return false;

				return LeaderMembership?.AthleteId != App.Instance.CurrentAthlete?.Id;
			}
		}

		public Membership LeaderMembership
		{
			get
			{
				return League?.Memberships.OrderBy(m => m.CurrentRank).FirstOrDefault();
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

		//public bool CanChallenge
		//{
		//	get
		//	{
		//		return GetBestChallengee != null;
		//	}
		//}

		//public Membership GetBestChallengee
		//{
		//	get
		//	{
		//		if(League == null)
		//			return null;
				
		//		if(!League.HasStarted && CurrentMembership != null)
		//			return null;

		//		var gap = League?.MaxChallengeRange;
		//		Membership best = null;
		//		while(best == null && gap > 0 && CurrentMembership != null)
		//		{
		//			var delta = CurrentMembership.CurrentRank - gap;
		//			best = League?.Memberships.SingleOrDefault(m => m.CurrentRank == delta);

		//			if(delta >= 0)
		//			{
		//				if(best == null)
		//					return null;

		//				//Ensure no issues with player
		//				var conflict = best.GetChallengeConflictReason(CurrentMembership.Athlete);
		//				if(best != null && conflict != null)
		//					best = null;
		//			}

		//			gap--;
		//		}

		//		return best;
		//	}
		//}

		public string DateRange
		{
			get
			{
				if(League == null)
					return null;

				var range = "open season";

				if(League != null && League.StartDate.HasValue)
					range = $"beginning {League?.StartDate.Value.ToString("MMM dd, yyyy")}";

				if(League != null && League.EndDate.HasValue)
					range += $"- {League?.EndDate.Value.ToString("MMM dd, yyyy")}";

				return range;
			}
		}

		public override void NotifyPropertiesChanged([CallerMemberName] string caller = "")
		{
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

	public class LeagueSortComparer : IComparer<LeagueViewModel>
	{
		public int Compare(LeagueViewModel x, LeagueViewModel y)
		{
			if(x?.League == null || y?.League == null)
				return -1;

			return string.Compare(x.League?.Name, y.League?.Name, System.StringComparison.OrdinalIgnoreCase);
		}
	}
}