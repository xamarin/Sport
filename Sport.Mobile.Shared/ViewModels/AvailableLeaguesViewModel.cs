using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Sport.Mobile.Shared
{
	public class AvailableLeaguesViewModel : BaseViewModel
	{
		ObservableCollection<LeagueViewModel> _leagues = new ObservableCollection<LeagueViewModel>();

		public ObservableCollection<LeagueViewModel> Leagues
		{
			get
			{
				return _leagues;
			}
			set
			{
				SetPropertyChanged(ref _leagues, value);
				Leagues?.Clear();
			}
		}

		public ICommand GetAvailableLeaguesCommand
		{
			get
			{
				return new Command(async () => await GetAvailableLeagues(true), () => {
					return !IsBusy; });
			}
		}

		async public Task GetAvailableLeagues(bool forceRefresh = false)
		{
			Debug.WriteLine(IsBusy);
			using(new Busy(this))
			{
				Debug.WriteLine(IsBusy);
				try
				{
					var leagueIds = App.Instance.CurrentAthlete.Memberships.Select(m => m.LeagueId).ToList();
					var toJoin = await AzureService.Instance.LeagueManager.Table.Where(l => l.IsAcceptingMembers && l.IsEnabled).ToListAsync();

					if(leagueIds.Count > 0)
					{
						toJoin = toJoin.Where(l => !leagueIds.Contains(l.Id)).ToList();
					}

					Leagues.Clear();
					toJoin.ForEach(l => Leagues.Add(new LeagueViewModel { League = l }));
				}
				catch(Exception e)
				{
					System.Diagnostics.Debug.WriteLine(e);
				}

				if(Leagues.Count == 0)
				{
					Leagues.Add(new LeagueViewModel
					{
						EmptyMessage = "There are no available leagues to join."
					});
				}
			}
			Debug.WriteLine(IsBusy);
		}
	}
}