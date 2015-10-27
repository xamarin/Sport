using Xamarin.Forms;

namespace Sport.Shared
{
	public partial class LeagueLandingPage : LeagueLandingXaml
	{
		public LeagueLandingPage()
		{
			Initialize();
		}

		async protected override void Initialize()
		{
			InitializeComponent();
			Title = "Leagues";

			btnAdd.Clicked += async(sender, e) =>
			{
				await Navigation.PushModalAsync(new NavigationPage(GetDetailsPage(null)));
			};

			list.ItemSelected += async(sender, e) =>
			{
				if(list.SelectedItem == null)
					return;

				var vm = list.SelectedItem as LeagueViewModel;
				list.SelectedItem = null;
				await Navigation.PushModalAsync(new NavigationPage(GetDetailsPage(vm.League)));
			};

			await ViewModel.GetAllLeagues();
		}

		LeagueEditPage GetDetailsPage(League league)
		{
			var detailsPage = new LeagueEditPage(league);
			detailsPage.OnUpdate = () =>
			{
				ViewModel.LocalRefresh();
				detailsPage.OnUpdate = null;
			};

			return detailsPage;
		}
	}

	public partial class LeagueLandingXaml : BaseContentPage<LeagueLandingViewModel>
	{
	}
}