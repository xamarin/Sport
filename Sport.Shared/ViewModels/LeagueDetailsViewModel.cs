using Xamarin.Forms;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Sport.Shared
{
	public class LeagueDetailsViewModel : LeagueViewModel
	{
		#region Properties

		MembershipDetailsViewModel _membershipViewModel;

		public MembershipDetailsViewModel MembershipViewModel
		{
			get
			{
				if(_membershipViewModel == null)
				{
					if(IsMember)
					{
						_membershipViewModel = new MembershipDetailsViewModel {
							MembershipId = App.CurrentAthlete.Memberships.First(m => m.LeagueId == League.Id).Id
						};
					}
				}

				return _membershipViewModel;
			}
		}

		public ChallengeDetailsViewModel OngoingChallengeViewModel
		{
			get;
			set;
		}

		#endregion

		async public Task LoadAthlete()
		{
			await RunSafe(AzureService.Instance.GetAthleteById(League.CreatedByAthleteId));
			League.RefreshMemberships();

			League.SetPropertyChanged("CreatedByAthlete");
			NotifyPropertiesChanged();
		}

		async public Task<bool> JoinLeague()
		{
			var membership = new Membership {
				AthleteId = App.CurrentAthlete.Id,
				LeagueId = League.Id,
				CurrentRank = 0,
			};

			var task = AzureService.Instance.SaveMembership(membership);
			await RunSafe(task);

			var theme = membership.League?.Theme;
			var getLeaderboardTask = AzureService.Instance.GetLeagueById(membership.LeagueId, true);
			await RunSafe(getLeaderboardTask);

			if(getLeaderboardTask.IsCompleted && !getLeaderboardTask.IsFaulted)
			{
				membership.League.Theme = theme;
				membership.League.LocalRefresh();
			}
				
			if(task.IsCompleted && !task.IsFaulted)
			{
				var regTask = AzureService.Instance.UpdateAthleteNotificationHubRegistration(App.CurrentAthlete, true);
				await RunSafe(regTask);
			}

			NotifyPropertiesChanged();
			return task.IsCompleted && !task.IsFaulted;
		}

		async public Task LeaveLeague()
		{
			var membership = App.CurrentAthlete.Memberships.SingleOrDefault(m => m.LeagueId == League.Id);

			var task = AzureService.Instance.DeleteMembership(membership.Id);
			await RunSafe(task);

			if(task.IsCompleted && !task.IsFaulted)
			{
				var regTask = AzureService.Instance.UpdateAthleteNotificationHubRegistration(App.CurrentAthlete, true);
				await RunSafe(regTask);
			}

			NotifyPropertiesChanged();
		}

		async public Task RefreshLeague(bool force = false)
		{
			if(IsBusy)
				return;

			using(new Busy(this))
			{
				var task = AzureService.Instance.GetLeagueById(League.Id, true);
				await RunSafe(task);

				if(task.IsFaulted)
					return;

				task.Result.Theme = League?.Theme;
				_praisePhrase = null;
				NotifyPropertiesChanged();
			}
		}

		public override void NotifyPropertiesChanged()
		{
			base.NotifyPropertiesChanged();

			CurrentMembership?.LocalRefresh();

			if(CurrentMembership?.OngoingChallenge == null)
				OngoingChallengeViewModel = null;

			if(CurrentMembership?.OngoingChallenge != null)
			{
				if(OngoingChallengeViewModel == null)
					OngoingChallengeViewModel = new ChallengeDetailsViewModel();

				OngoingChallengeViewModel.Challenge = CurrentMembership.OngoingChallenge;
			}

			SetPropertyChanged("OngoingChallengeViewModel");
			SetPropertyChanged("PreviousChallengeViewModel");

			MembershipViewModel?.NotifyPropertiesChanged();
			OngoingChallengeViewModel?.NotifyPropertiesChanged();
		}
	}
}