using Xamarin.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

//using Google.Apis.Calendar.v3.Data;
//using Google.Apis.Calendar.v3;

namespace Sport.Shared
{
	public class ChallengeDetailsViewModel : ChallengeViewModel
	{
		async public Task GetMatchResults(bool forceRefresh = false)
		{
			if(!forceRefresh && Challenge.MatchResult.Count > 0)
				return;

			var task = new Task<List<GameResult>>(() => AzureService.Instance.Client.GetTable<GameResult>().Where(r => r.ChallengeId == Challenge.Id).OrderBy(r => r.Index).ToListAsync().Result);
			await RunSafe(task);

			if(task.IsFaulted)
				return;

			var results = task.Result;

			Challenge.MatchResult.Clear();
			results.ForEach(Challenge.MatchResult.Add);
			SetPropertyChanged("Challenge");
		}

		async public Task<bool> AcceptChallenge()
		{
			var task = AzureService.Instance.AcceptChallenge(Challenge);
			await RunSafe(task);

			if(task.IsCompleted && !task.IsFaulted)
			{
				task = AddChallengeEventToCalendar();
				await RunSafe(task);
			}

			NotifyPropertiesChanged();
			MessagingCenter.Send<App>(App.Current, Messages.ChallengesUpdated);
			return !task.IsFaulted;
		}

		async public Task<bool> DeclineChallenge()
		{
			Task task = null;
			if(App.CurrentAthlete.Id == Challenge.ChallengerAthleteId)
			{
				task = AzureService.Instance.RevokeChallenge(Challenge.Id);
			}
			else if(App.CurrentAthlete.Id == Challenge.ChallengeeAthleteId)
			{
				task = AzureService.Instance.DeclineChallenge(Challenge.Id);
			}

			await RunSafe(task);

			Challenge.League.RefreshChallenges();
			MessagingCenter.Send<App>(App.Current, Messages.ChallengesUpdated);
			return !task.IsFaulted;
		}

		public Task AddChallengeEventToCalendar()
		{
			return new Task(() =>
			{
//				var service = new CalendarService();
//				service.HttpClient.DefaultRequestHeaders.Add("Authorization", App.AuthTokenAndType);
//				var req = service.CalendarList.List();
//				var list = req.Execute();
//
//				var primaryCalendar = list.Items.ToList().FirstOrDefault(i => i.Primary.HasValue && i.Primary.Value);
//
//				if(primaryCalendar == null)
//				{
//					"Unable to locate default calendar".ToToast();
//					return;
//				}
//
//				var evnt = new Event();
//				evnt.Attendees = new List<EventAttendee> {
//					new EventAttendee {
//							Email = Opponent.Email,
//							DisplayName = Opponent.Name,
//						}
//				};
//
//				evnt.Summary = "{0}: {1} vs {2}".Fmt(Challenge.League.Name, Challenge.ChallengerAthlete.Alias, Challenge.ChallengeeAthlete.Alias);
//				evnt.Description = Challenge.BattleForPlaceBetween;
//				evnt.Start = new EventDateTime {
//					DateTime = Challenge.ProposedTime.UtcDateTime,
//					TimeZone = "GMT",
//				};
//
//				evnt.End = new EventDateTime {
//					DateTime = Challenge.ProposedTime.UtcDateTime.AddMinutes(30),
//					TimeZone = "GMT",
//				};
//
//				service.Events.Insert(evnt, primaryCalendar.Id).Execute();
			});
		}

		async public Task NudgeAthlete()
		{
			using(new Busy(this))
			{
				var task = AzureService.Instance.NagAthlete(Challenge.Id);
				await RunSafe(task);

				if(task.IsFaulted)
					return;
			}
		}

		async public Task RefreshChallenge()
		{
			if(Challenge == null)
				return;
			
			using(new Busy(this))
			{
				var task = AzureService.Instance.GetChallengeById(Challenge.Id, true);
				await RunSafe(task);

				if(task.IsFaulted)
				{
					Challenge = null;
					return;
				}

				if(task.Result != Challenge)
					Challenge = task.Result;
			}
		}
	}
}