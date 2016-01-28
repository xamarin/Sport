using System.Threading.Tasks;
using Xamarin.Forms;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;

namespace Sport.Shared
{
	public class AthleteLeaguesViewModel : BaseViewModel
	{
		bool _hasLoadedLeaguesBefore;
		string _athleteId;

		public AthleteViewModel AthleteViewModel
		{
			get;
			set;
		}

		public string AthleteId
		{
			get
			{
				return _athleteId;
			}
			set
			{
				_athleteId = value;
				SetPropertyChanged("Athlete");
				AthleteViewModel = new AthleteViewModel {
					AthleteId = Athlete?.Id
				};
			}
		}

		public Athlete Athlete
		{
			get
			{
				return AthleteId == null ? null : DataManager.Instance.Athletes.Get(AthleteId);
			}
		}

		public ICommand GetLeaguesCommand
		{
			get
			{
				return new Command(async() => await RemoteRefresh());
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

		async public Task GetLeagues(bool forceRefresh = false)
		{
			if(_hasLoadedLeaguesBefore && !forceRefresh)
			{
				Athlete.LocalRefresh();
				return;
			}

			using(new Busy(this))
			{
				await AthleteViewModel.GetLeagues(forceRefresh);
				LocalRefresh();

				//Settings.Instance.LeagueColors.Clear();
				DataManager.Instance.Leagues.Values.ToList().EnsureLeaguesThemed();
			}

			_hasLoadedLeaguesBefore = true;
		}

		public async Task RemoteRefresh()
		{
			await GetLeagues(true);
		}

		LeagueViewModel _empty;

		public void LocalRefresh()
		{
			if(Athlete == null)
				return;

			if(Leagues == null)
				Leagues = new ObservableCollection<LeagueViewModel>();

			var comparer = new LeagueIdComparer();
			var toRemove = Leagues.Where(vm => vm.League != null).Select(vm => vm.League).Except(Athlete.Leagues, comparer).ToList();
			var toAdd = Athlete.Leagues.Except(Leagues.Select(vm => vm.League), comparer).OrderBy(r => r.Name).Select(l => new LeagueViewModel {
				LeagueId = l.Id
			}).ToList();

			toRemove.ForEach(l => Leagues.Remove(Leagues.Single(vm => vm.League == l)));
			foreach(var l in Leagues)
				l.NotifyPropertiesChanged();

			if(Leagues.Count == 0 && toAdd.Count == 0)
			{
				if(_empty == null)
					_empty = new LeagueViewModel {
						EmptyMessage = "You don't belong to any leagues yet.\n\nYou can join leagues by tapping the + button above."
					};

				if(!Leagues.Contains(_empty))
					Leagues.Add(_empty);
			}

			var compare = new LeagueSortComparer();
			foreach(var lv in toAdd)
			{
				int index = 0;
				foreach(var l in Leagues.ToList())
				{
					if(compare.Compare(lv, l) < 0)
						break;

					index++;
				}
				Leagues.Insert(index, lv);
			}

			if(toAdd.Count > 0 || toRemove.Count > 0)
			{
				var last = Leagues.LastOrDefault();
				foreach(var l in Leagues)
					l.IsLast = l == last;
			}

			if(Leagues.Count > 0 && Leagues.Contains(_empty) && Leagues.First() != _empty)
			{
				Leagues.Remove(_empty);
			}
		}
	}
}