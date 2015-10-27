using Xamarin.Forms;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Sport.Shared
{
	public partial class AvailableLeaguesPage : AvailableLeaguesXaml
	{
		public AvailableLeaguesPage()
		{
			Initialize();
		}

		public Action<League> OnJoinedLeague
		{
			get;
			set;
		}

		async protected override void Initialize()
		{
			InitializeComponent();
			Title = "Available Leagues";

			list.ItemSelected += OnItemSelected;
			AddDoneButton();

			await ViewModel.GetAvailableLeagues(true);
		}

		async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			if(list.SelectedItem == null)
				return;

			var vm = list.SelectedItem as LeagueViewModel;
			list.SelectedItem = null;

			//Empty message
			if(vm.LeagueId == null)
				return;

			var page = new LeagueDetailsPage(vm.League);

			page.OnJoinedLeague = (l) =>
			{
				ViewModel.LocalRefresh();
				if(OnJoinedLeague != null)
				{
					OnJoinedLeague(l);
				}

				Device.BeginInvokeOnMainThread(() =>
				{
					Navigation.PopAsync();
				});
			};

			await Navigation.PushAsync(page);
		}

		protected override void OnDisappearing()
		{
			ViewModel.CancelTasks();
			base.OnDisappearing();
		}
	}

	public partial class AvailableLeaguesXaml : BaseContentPage<AvailableLeaguesViewModel>
	{
	}
}