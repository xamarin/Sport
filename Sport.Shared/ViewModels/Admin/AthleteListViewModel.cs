using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Windows.Input;
using System.Linq;

namespace Sport.Shared
{
	public class AthleteListViewModel : BaseViewModel
	{
		bool _hasLoadedBefore;

		public AthleteListViewModel()
		{
			AllAthletes = new ObservableCollection<Athlete>();
		}

		public ObservableCollection<Athlete> AllAthletes
		{
			get;
			set;
		}

		public ICommand GetAllAthletesCommand
		{
			get
			{
				return new Command(async() => await GetAllAthletes(true));
			}
		}

		public void LocalRefresh()
		{
			AllAthletes.Clear();
			DataManager.Instance.Athletes.Values.OrderBy(a => a.Name).ToList().ForEach(AllAthletes.Add);
		}

		async public Task GetAllAthletes(bool forceRefresh = false)
		{
			if(_hasLoadedBefore && !forceRefresh)
				return;
			
			AllAthletes.Clear();

			var task = AzureService.Instance.GetAllAthletes();
			await RunSafe(task);

			if(task.IsFaulted)
				return;

			_hasLoadedBefore = true;
			LocalRefresh();
		}
	}
}