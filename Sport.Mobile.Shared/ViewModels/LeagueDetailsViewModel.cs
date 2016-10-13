using Xamarin.Forms;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Sport.Mobile.Shared
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
						_membershipViewModel = new MembershipDetailsViewModel
						{
							Membership = App.Instance.CurrentAthlete.Memberships.First(m => m.LeagueId == League.Id)
						};
					}
				}

				return _membershipViewModel;
			}
		}

		public ObservableCollection<ChallengeDetailsViewModel> OngoingChallengeViewModels
		{
			get;
			set;
		}

		public override League League
		{
			get
			{
				return base.League;
			}
			set
			{
				base.League = value;
				SetOngoingChallenges();
			}
		}
		

		#endregion

		async public Task<bool> JoinLeague()
		{
			var membership = new Membership {
				AthleteId = App.Instance.CurrentAthlete.Id,
				LeagueId = League.Id,
			};

			var success = await AzureService.Instance.MembershipManager.InsertAsync(membership);

			if(success)
			{
				App.Instance.CurrentAthlete.LocalRefresh();
				membership.LocalRefresh();
			}

			return success;
		}

		async public Task LeaveLeague()
		{
			var membership = App.Instance.CurrentAthlete.Memberships.SingleOrDefault(m => m.LeagueId == League.Id);

			await AzureService.Instance.MembershipManager.RemoveAsync(membership);
			await AzureService.Instance.ChallengeManager.PullLatestAsync();

			NotifyPropertiesChanged();
		}

		async public Task RefreshLeague(bool force = false)
		{
			if(IsBusy)
				return;

			using(new Busy(this))
			{
				await AzureService.Instance.LeagueManager.GetItemAsync(League.Id, true);
				await AzureService.Instance.ChallengeManager.PullLatestAsync();
				await AzureService.Instance.AthleteManager.PullLatestAsync();

				_praisePhrase = null;
				NotifyPropertiesChanged();
			}
		}

		void SetOngoingChallenges()
		{
			if(OngoingChallengeViewModels == null)
				OngoingChallengeViewModels = new ObservableCollection<ChallengeDetailsViewModel>();

			OngoingChallengeViewModels.Clear();

			if(CurrentMembership != null)
			{
				foreach(var challenge in CurrentMembership.OngoingChallenges)
				{
					OngoingChallengeViewModels.Add(new ChallengeDetailsViewModel { Challenge = challenge });
				}
			}
		}

		public override void NotifyPropertiesChanged([CallerMemberName] string caller = "")
		{
			Debug.WriteLine($"Notify called for League - {League?.Name} by {caller}");
			base.NotifyPropertiesChanged();

			App.Instance.CurrentAthlete.LocalRefresh();
			League.LocalRefresh();

			CurrentMembership?.LocalRefresh();
			SetPropertyChanged("CurrentMembership");

			if(CurrentMembership?.OngoingChallenges == null)
			{
				OngoingChallengeViewModels = new ObservableCollection<ChallengeDetailsViewModel>();
			}
			else if(CurrentMembership?.OngoingChallenges != null)
			{
				SetOngoingChallenges();
			}
			else
			{
				SetPropertyChanged("OngoingChallengeViewModels");
				OngoingChallengeViewModels.ForEach(vm => vm.NotifyPropertiesChanged());
			}

			MembershipViewModel?.NotifyPropertiesChanged();
		}
	}
}