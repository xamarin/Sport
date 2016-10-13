using System.Threading.Tasks;
using Xamarin.Forms;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Diagnostics;

namespace Sport.Mobile.Shared
{
	public class AthleteLeaguesViewModel : BaseViewModel
	{
		public Athlete Athlete
		{
			get;
			set;
		}

		public ICommand GetLeaguesCommand
		{
			get
			{
				return new Command(async() => await GetLeaguesForAthlete(true));
			}
		}

		public ObservableCollection<LeagueViewModel> Leagues
		{
			get;
			set;
		}

		public AthleteLeaguesViewModel()
		{
			Leagues = new ObservableCollection<LeagueViewModel>();
		}

		async public Task GetLeaguesForAthlete(bool forceRefresh = false)
		{
			using(new Busy(this))
			{
				var leagues = await AzureService.Instance.LeagueManager.GetItemsAsync(forceRefresh);
				await AzureService.Instance.MembershipManager.GetItemsAsync(forceRefresh);
				await AzureService.Instance.AthleteManager.GetItemsAsync(forceRefresh);

				App.Instance.CurrentAthlete.LocalRefresh();

				var leagueIds = App.Instance.CurrentAthlete.Memberships.Select(m => m.LeagueId);
				var joined = leagues.Where(l => leagueIds.Contains(l.Id)).OrderBy(l => l.Name).ToList();

				if(joined != null)
				{
					Leagues.Clear();
					foreach(var i in joined)
					{
						var l = new LeagueViewModel { League = i };
						Debug.WriteLine(i.Name);
						Leagues.Add(l);
					}

					if(Leagues.Count == 0)
					{
						Leagues.Add(new LeagueViewModel
						{
							EmptyMessage = "You don't belong to any leagues.\n\nClick the + button to join one."
						});
					}
				}
			}
		}
	}
}