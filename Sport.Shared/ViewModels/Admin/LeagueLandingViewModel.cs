using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Sport.Shared
{
	public class LeagueLandingViewModel : BaseViewModel
	{
		bool _hasLoadedBefore;

		public LeagueLandingViewModel()
		{
			LocalRefresh();
		}

		ObservableCollection<LeagueViewModel> _allLeagues = new ObservableCollection<LeagueViewModel>();

		public ObservableCollection<LeagueViewModel> AllLeagues
		{
			get
			{
				return _allLeagues;
			}
			set
			{
				SetPropertyChanged(ref _allLeagues, value);
			}
		}

		public ICommand GetAllLeaguesCommand
		{
			get
			{
				return new Command(async() => await GetAllLeagues(true));
			}
		}

		public void LocalRefresh()
		{
			AllLeagues.Clear();
			DataManager.Instance.Leagues.Values.OrderBy(l => l.Name).ToList().ForEach(l => AllLeagues.Add(new LeagueViewModel {
				LeagueId = l.Id
			}));
		}

		async public Task GetAllLeagues(bool forceRefresh = false)
		{
			if(_hasLoadedBefore && !forceRefresh)
				return;

			using(new Busy(this))
			{
				AllLeagues.Clear();
				await RunSafe(AzureService.Instance.GetAllLeagues());
				_hasLoadedBefore = true;
				LocalRefresh();
			}
		}
	}
}