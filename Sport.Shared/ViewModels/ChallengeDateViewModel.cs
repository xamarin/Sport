using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sport.Shared
{
	public class ChallengeDateViewModel : ChallengeViewModel
	{
		public DateTime SelectedDateTime
		{
			get
			{
				return SelectedDate.Add(SelectedTime);
			}
		}

		DateTime _selectedDate;

		public DateTime SelectedDate
		{
			get
			{
				return _selectedDate;
			}
			set
			{
				SetPropertyChanged(ref _selectedDate, value);
				SetPropertyChanged("SelectedDateTime");
			}
		}

		TimeSpan _selectedTime;

		public TimeSpan SelectedTime
		{
			get
			{
				return _selectedTime;
			}
			set
			{
				SetPropertyChanged(ref _selectedTime, value);
				SetPropertyChanged("SelectedDateTime");
			}
		}

		async public Task<Challenge> PostChallenge()
		{
			Challenge.ProposedTime = SelectedDateTime.ToUniversalTime();				
			var task = AzureService.Instance.SaveChallenge(Challenge);
			await RunSafe(task);

			if(task.IsFaulted)
				return null;

			Challenge.League.RefreshChallenges();
			MessagingCenter.Send<App>(App.Current, Messages.ChallengesUpdated);
			return Challenge;
		}

		public void CreateChallenge(Athlete challenger, Athlete challengee, League league)
		{
			var time = TimeSpan.FromTicks(DateTime.Now.AddMinutes(60).Subtract(DateTime.Today).Ticks);

			if(time.Ticks > TimeSpan.TicksPerDay)
				time = time.Subtract(TimeSpan.FromTicks(TimeSpan.TicksPerDay));

			SelectedTime = time;
			SelectedDate = DateTime.Today;

			var membership = league.Memberships.SingleOrDefault(m => m.AthleteId == challengee.Id);

			Challenge = new Challenge {
				BattleForRank = membership.CurrentRank,
				ChallengerAthleteId = challenger.Id,
				ChallengeeAthleteId = challengee.Id,
				ProposedTime = SelectedDateTime,
				LeagueId = league.Id,
			};
		}

		public string Validate()
		{
			var sb = new StringBuilder();

			if(SelectedDate.AddTicks(SelectedTime.Ticks) <= DateTime.Now.AddMinutes(5))
			{
				sb.AppendLine("Please choose a date at least 5 minutes from now.");
			}

			return sb.Length == 0 ? null : sb.ToString();
		}
	}
}