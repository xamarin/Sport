
namespace Sport.Shared
{
	public class MembershipViewModel : BaseViewModel
	{
		string _membershipId;

		public string MembershipId
		{
			get
			{
				return _membershipId;
			}
			set
			{
				SetPropertyChanged(ref _membershipId, value);
				_membership = null;
				SetPropertyChanged("Membership");
			}
		}

		Membership _membership;

		public Membership Membership
		{
			get
			{
				if(_membership == null)
					_membership = MembershipId == null ? null : DataManager.Instance.Memberships.Get(MembershipId);

				return _membership;
			}
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
				return Membership != null && Membership.AthleteId == App.CurrentAthlete.Id;
			}
		}

		public bool CanDeleteMembership
		{
			get
			{
				return App.CurrentAthlete.IsAdmin && Membership.Id != null;
			}
		}

		public bool CanRevokeChallenge
		{
			get
			{
				var challenge = Membership.GetOngoingChallenge(App.CurrentAthlete);
				return challenge != null && challenge.ChallengerAthleteId == App.CurrentAthlete.Id && challenge.ChallengeeAthleteId == Membership.AthleteId
				&& Membership.LeagueId == challenge.LeagueId;
			}
		}

		public bool CanChallenge
		{
			get
			{
				return Membership.CanChallengeAthlete(App.CurrentAthlete);
			}
		}

		public string JoinDescription
		{
			get
			{
				return "joined {0} on {1}".Fmt(Membership.League.Name, Membership.DateCreated.Value.ToString("M"));
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

		public override void NotifyPropertiesChanged()
		{
			_membership = null;

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