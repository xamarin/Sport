
using System.Diagnostics;

namespace Sport.Mobile.Shared
{
	public class MembershipViewModel : BaseViewModel
	{
		public Membership Membership
		{
			get;
			set;
		}

		public string Alias
		{
			get
			{
				return IsCurrentMembership ? "*You*" : Membership != null ? Membership.Athlete.Alias : null;
			}
		}

		public bool IsFirstPlace
		{
			get
			{
				return Membership != null && Membership.CurrentRank == 0 && Membership.League != null && Membership.League.HasStarted;
			}
		}

		public bool IsCurrentMembership
		{
			get
			{
				return Membership != null && Membership.AthleteId == App.Instance.CurrentAthlete.Id;
			}
		}

		public bool CanDeleteMembership
		{
			get
			{
				return App.Instance.CurrentAthlete.IsAdmin && Membership.Id != null;
			}
		}

		public bool CanRevokeChallenge
		{
			get
			{
				var challenge = Membership.GetOngoingChallenge(App.Instance.CurrentAthlete);
				return challenge != null && challenge.ChallengerAthleteId == App.Instance.CurrentAthlete.Id && challenge.ChallengeeAthleteId == Membership.AthleteId
				&& Membership.LeagueId == challenge.LeagueId;
			}
		}

		public bool CanChallenge
		{
			get
			{
				return Membership.CanChallengeAthlete(App.Instance.CurrentAthlete);
			}
		}

		public string JoinDescription
		{
			get
			{
				return "joined {0} on {1}".Fmt(Membership.League.Name, Membership.CreatedAt.Value.ToString("M"));
			}
		}

		public string Stats
		{
			get
			{
				return "W 14 L 7 ~ .5 (mock)";
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

		public override void NotifyPropertiesChanged([System.Runtime.CompilerServices.CallerMemberName] string caller = "")
		{
			base.NotifyPropertiesChanged();

			SetPropertyChanged("Membership");
			SetPropertyChanged("IsCurrentMemebership");
			SetPropertyChanged("EmptyMessage");
			SetPropertyChanged("IsFirstPlace");
			SetPropertyChanged("CanChallenge");
			SetPropertyChanged("CanRevokeChallenge");
			SetPropertyChanged("Membership");
			SetPropertyChanged("CanDeleteMembership");
			SetPropertyChanged("RankDescription");
			SetPropertyChanged("Stats");
			SetPropertyChanged("JoinDescription");
		}

		public void LocalRefresh()
		{
			Membership.Athlete.LocalRefresh();
			Membership?.LocalRefresh();
		}
	}
}