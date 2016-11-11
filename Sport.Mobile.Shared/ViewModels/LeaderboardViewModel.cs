using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using System;
using Xamarin;

namespace Sport.Mobile.Shared
{
	public class LeaderboardViewModel : BaseViewModel
	{
		string _leagueId;
		League _league;

		public League League
		{
			get
			{
				if(_league == null)
				{
						Task.Run(async () => {
							_league = await AzureService.Instance.LeagueManager.GetItemAsync(_leagueId);
						}).Wait();
				}

				return _league;
			}
			set
			{
				_league = value;
				_leagueId = value?.Id;
			}
		}

		public ObservableCollection<MembershipViewModel> Memberships
		{
			get;
			set;
		} = new ObservableCollection<MembershipViewModel>();

		public ICommand GetLeaderboardCommand
		{
			get
			{
				return new Command(async() => await GetLeaderboard(true));
			}
		}

		async public Task GetLeaderboard(bool forceRefresh = false)
		{
			using(new Busy(this))
			{
				await AzureService.Instance.LeagueManager.GetItemAsync(League.Id, forceRefresh);

				_league = null;
				League.LocalRefresh();

				Memberships.Clear();
				League.Memberships.ForEach(i => Memberships.Add(new MembershipViewModel { Membership = i }));

				if(Memberships.Count == 0)
				{
					Memberships.Add(new MembershipViewModel
					{
						EmptyMessage = "This league has no members yet"
					});
				}
			}
		}

		async public Task LocalRefresh(bool forceRefresh = false)
		{
			await GetLeaderboard();
		}
	}
}