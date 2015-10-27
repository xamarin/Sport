using Xamarin.Forms;

namespace Sport.Shared
{
	public partial class AthleteListPage : AthleteListPageXaml
	{
		public AthleteListPage()
		{
			Initialize();
		}

		async protected override void Initialize()
		{
			InitializeComponent();
			Title = "Athletes";

			list.ItemSelected += async(sender, e) =>
			{
				if(list.SelectedItem == null)
					return;

				var athlete = list.SelectedItem as Athlete;
				list.SelectedItem = null;

				var detailsPage = new AthleteProfilePage(athlete.Id);
				detailsPage.OnSave = () =>
				{
					ViewModel.LocalRefresh();
					detailsPage.OnSave = null;
				};

				await Navigation.PushAsync(detailsPage);
			};

			using(new HUD("Getting all athletes..."))
			{
				await ViewModel.GetAllAthletes();
			}
		}
	}

	public partial class AthleteListPageXaml : BaseContentPage<AthleteListViewModel>
	{
	}
}