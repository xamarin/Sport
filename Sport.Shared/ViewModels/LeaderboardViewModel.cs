using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using System;
using Xamarin;

namespace Sport.Shared
{
	public class LeaderboardViewModel : BaseViewModel
	{
		bool _hasLoadedBefore;

		League _league;

		public League League
		{
			get
			{
				return _league;
			}
			set
			{
				if(value != _league)
				{
					_hasLoadedBefore = false;
					Memberships.Clear();
				}

				SetPropertyChanged(ref _league, value);
			}
		}

		public ObservableCollection<MembershipViewModel> Memberships
		{
			get;
			set;
		}

		public LeaderboardViewModel()
		{
			Memberships = new ObservableCollection<MembershipViewModel>();
		}

		public ICommand GetLeaderboardCommand
		{
			get
			{
				return new Command(async() => await GetLeaderboard(true));
			}
		}

		async public Task GetLeaderboard(bool forceRefresh = false)
		{
			if(!forceRefresh && _hasLoadedBefore)
				return;

			using(new Busy(this))
			{
				var task = AzureService.Instance.GetLeagueById(League.Id, true);
				await RunSafe(task);

				if(task.IsFaulted)
					return;

				LocalRefresh();
			}
		}

		public void LocalRefresh()
		{
			try
			{
				League.LocalRefresh();

				var memberships = Memberships.Select(vm => vm.Membership).ToList();

				var comparer = new MembershipComparer();
				var toRemove = memberships.Except(League.Memberships, comparer).ToList();
				var toAdd = League.Memberships.Except(memberships, comparer).ToList();

				toRemove.ForEach(m => Memberships.Remove(Memberships.Single(vm => vm.Membership == m)));

				toAdd.ForEach(m => Memberships.Add(new MembershipViewModel {
					MembershipId = m.Id
				}));

				Memberships.Sort(new MembershipSortComparer());
				Memberships.ToList().ForEach(vm => vm.NotifyPropertiesChanged());

				if(Memberships.Count == 0)
				{
					Memberships.Add(new MembershipViewModel {
						EmptyMessage = "This league has no members yet"
					});
				}
			}
			catch(Exception e)
			{
				InsightsManager.Report(e);
			}
		}
	}
}