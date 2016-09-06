using Xamarin.Forms;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;

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

		public ChallengeDetailsViewModel OngoingChallengeViewModel
		{
			get;
			set;
		}

		#endregion

		async public Task<bool> JoinLeague()
		{
			var membership = new Membership {
				AthleteId = App.Instance.CurrentAthlete.Id,
				LeagueId = League.Id,
				CurrentRank = 0,
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
			App.Instance.CurrentAthlete.LocalRefresh();

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

				_praisePhrase = null;
				NotifyPropertiesChanged();
			}
		}

		public override void NotifyPropertiesChanged([CallerMemberName] string caller = "")
		{
			Debug.WriteLine($"Notify called for League - {League?.Name} by {caller}");
			base.NotifyPropertiesChanged();

			App.Instance.CurrentAthlete.LocalRefresh();
			League.LocalRefresh();

			SetPropertyChanged("CurrentMembership");
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

			MembershipViewModel?.NotifyPropertiesChanged();
			OngoingChallengeViewModel?.NotifyPropertiesChanged();
		}
	}
}