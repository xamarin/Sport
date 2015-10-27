
namespace Sport.Shared
{
	public class ChallengeViewModel : BaseViewModel
	{
		protected Challenge _challenge;

		public Challenge Challenge
		{
			get
			{
				return _challenge;
			}
			set
			{
				SetPropertyChanged(ref _challenge, value);
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

		public bool CanAccept
		{
			get
			{
				return Challenge != null && Challenge.ChallengeeAthleteId == App.CurrentAthlete.Id && !_challenge.IsAccepted && !Challenge.IsCompleted;
			}
		}

		public bool CanDecline
		{
			get
			{
				return CanAccept;
			}
		}

		public bool CanDeclineAfterAccept
		{
			get
			{
				return Challenge != null && Challenge.ChallengeeAthleteId == App.CurrentAthlete.Id && Challenge.IsAccepted && !Challenge.IsCompleted;
			}
		}

		public bool CanRevoke
		{
			get
			{
				return Challenge != null && Challenge.ChallengerAthleteId == App.CurrentAthlete.Id && !Challenge.IsCompleted;
			}
		}

		public bool CanPostMatchResults
		{
			get
			{
				return Challenge != null && Challenge.IsAccepted && !Challenge.IsCompleted && Challenge.InvolvesAthlete(App.CurrentAthlete.Id);
			}
		}

		public bool AwaitingDecision
		{
			get
			{
				return Challenge != null && !CanAccept && !CanPostMatchResults && Challenge.InvolvesAthlete(App.CurrentAthlete.Id) && !Challenge.IsCompleted;
			}
		}

		public Athlete Opponent
		{
			get
			{
				return Challenge != null && Challenge.ChallengeeAthleteId != App.CurrentAthlete.Id ? Challenge?.ChallengeeAthlete : Challenge?.ChallengerAthlete;
			}
		}

		public string ChallengeStatus
		{
			get
			{
				if(Challenge == null)
					return null;

				string status = null;

				if(CanAccept)
					return "do you accept this honorable duel?";

				if(CanPostMatchResults)
					return "this is where you'll reflect upon your victorious match score... but you'll have to post some results first";

				if(CanRevoke)
					return "...just waiting for your opponent to accept your challenge or coward out like a shameless hunk of slime";

				if(!Challenge.IsCompleted)
					return "the results of this might duel have not yet been posted... check back soon for some tasty scores!";

				return status;
			}
		}

		public override void NotifyPropertiesChanged()
		{
			SetPropertyChanged("Challenge");
			SetPropertyChanged("EmptyMessage");
			SetPropertyChanged("CanAccept");
			SetPropertyChanged("CanDecline");
			SetPropertyChanged("CanDeclineAfterAccept");
			SetPropertyChanged("CanRevoke");
			SetPropertyChanged("CanPostMatchResults");
			SetPropertyChanged("Challenge");
			SetPropertyChanged("ChallengeStatus");
			SetPropertyChanged("Opponent");
			SetPropertyChanged("AwaitingDecision");

			Challenge?.NotifyPropertiesChanged();
		}
	}
}