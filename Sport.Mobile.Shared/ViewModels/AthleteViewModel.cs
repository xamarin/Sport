using System.Threading.Tasks;

namespace Sport.Mobile.Shared
{
	public class AthleteViewModel : BaseViewModel
	{
		string _athleteId;

		public string AthleteId
		{
			get
			{
				return _athleteId;
			}
			set
			{
				_athleteId = value;
				_athlete = null;
				SetPropertyChanged("Athlete");
			}
		}

		Athlete _athlete;

		public Athlete Athlete
		{
			get
			{
				if(_athlete == null && AthleteId != null)
				{
					Task.Run(async () => {
						_athlete = await AzureService.Instance.AthleteManager.Table.LookupAsync(AthleteId);
					}).Wait();
				}
				return _athlete;
			}
		}

		//async public Task GetLeagues(bool forceRefresh = false)
		//{
		//	if(Athlete == null)
		//		return;

		//	if(!forceRefresh)
		//	{
		//		Athlete.LocalRefresh();
		//		return;
		//	}

		//	if(IsBusy)
		//		return;

		//	using(new Busy(this))
		//	{
		//		Athlete.LocalRefresh();

		//		var task = AzureService.Instance.GetAllLeaguesForAthlete();
		//		await RunSafe(task);

		//		if(task.IsFaulted)
		//			return;

		//		Athlete.LocalRefresh();
		//		SetPropertyChanged("Athlete");
		//	}

		//	IsBusy = false;
		//}

		public override void NotifyPropertiesChanged([System.Runtime.CompilerServices.CallerMemberName] string caller = "")
		{
			_athlete = null;
			SetPropertyChanged("Athlete");
			base.NotifyPropertiesChanged();
		}
	}
}

