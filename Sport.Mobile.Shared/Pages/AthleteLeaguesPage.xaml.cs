using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Sport.Mobile.Shared
{
	public partial class AthleteLeaguesPage : AthleteLeaguesXaml
	{
		public AthleteLeaguesPage ()
		{
			Initialize ();
		}

		public AthleteLeaguesPage(Athlete athlete)
		{
			//ViewModel is newed up in the ViewModel getter of BaseContentPage
			ViewModel.Athlete = athlete;
			Initialize();
		}

		protected override void Initialize()
		{
			InitializeComponent();
			Title = "Leagues";
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			btnJoin.Clicked += OnJoinClicked;
			list.ItemSelected += OnListItemSelected;

			//foreach(var l in ViewModel.Leagues)
			//	l.NotifyPropertiesChanged();
		}

		public async override Task<bool> EnsureUserAuthenticated()
		{
			var success = await base.EnsureUserAuthenticated();

			if(success)
				await LoadLeagues();

			return success;
		}

		async public Task LoadLeagues()
		{
			await ViewModel.GetLeaguesForAthlete();
		}

		protected override void OnDisappearing()
		{
			btnJoin.Clicked -= OnJoinClicked;
			list.ItemSelected -= OnListItemSelected;

			base.OnDisappearing();
		}

		async void OnJoinClicked(object sender, EventArgs e)
		{
			var weakSelf = new WeakReference(this);
			var page = new AvailableLeaguesPage();
			page.OnJoinedLeague = async (l) => {
				var self = (AthleteLeaguesPage)weakSelf.Target;
				if(self == null)
					return;

				await self.ViewModel.GetLeaguesForAthlete();
				self.ViewModel.SetPropertyChanged("Athlete");
			};

			await Navigation.PushModalAsync(page.WithinNavigationPage());
		}

		async void OnListItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			//This event gets triggered when you set SelectedItem to null or the item is deselected by the user so we need to check for null first
			if(list.SelectedItem == null)
				return;

			var vm = list.SelectedItem as LeagueViewModel;
			list.SelectedItem = null; //Deselect the item

			if(vm.LeagueId == null) //Ensure the league is a valid league - some items in this list are used to display an empty message and do not have a LeagueId
				return;

			//Referencing 'this' in the body of delegate will prevent the page from being collected once it's popped
			var weakSelf = new WeakReference(this);
			var page = new LeagueDetailsPage(vm.League);
			page.OnAbandondedLeague = (async (l) => {
				var self = (AthleteLeaguesPage)weakSelf.Target;
				if(self == null)
					return;

				await self.ViewModel.GetLeaguesForAthlete();
				self.ViewModel.SetPropertyChanged("Athlete");
				await self.Navigation.PopAsync();
			});

			await Navigation.PushAsync(page);
		}

		protected override void OnIncomingPayload(NotificationPayload payload)
		{
			base.OnIncomingPayload(payload);

			string leagueId = null;
			if(payload.Payload.TryGetValue("leagueId", out leagueId))
			{
				//await ViewModel.RemoteRefresh();
				return;
			}

			string challengeId;
			if(payload.Payload.TryGetValue("challengeId", out challengeId))
			{
				//await ViewModel.RemoteRefresh();
				return;
			}
		}

		const string _admin = "Admin";
		const string _profile = "My Profile";
		const string _logout = "Log Out";
		const string _about = "About";

		List<string> GetMoreMenuOptions()
		{
			var lst = new List<string>();
			lst.Add(_profile);

			//			if(App.Instance.CurrentAthlete.IsAdmin)
			//				lst.Add(_admin);

			lst.Add(_about);
			lst.Add(_logout);

			return lst;
		}

		async void OnMoreClicked(object sender, EventArgs e)
		{
			var lst = GetMoreMenuOptions();
			var action = await DisplayActionSheet("Additional actions", "Cancel", null, lst.ToArray());

			if(action == _logout)
				OnLogoutSelected();

			if(action == _profile)
				OnProfileSelected();

			//if(action == _admin)
			//	OnAdminSelected();

			if(action == _about)
				OnAboutSelected();
		}

		void OnLogoutSelected()
		{
			LogoutUser();
		}

		async void OnProfileSelected()
		{
			if(App.Instance.CurrentAthlete == null || App.Instance.CurrentAthlete.Id == null)
				return;

			var page = new AthleteProfilePage(App.Instance.CurrentAthlete.Id);
			page.OnSave = async () => await Navigation.PopModalAsync();
			await Navigation.PushModalAsync(page.WithinNavigationPage());
		}

		//async void OnAdminSelected()
		//{
		//	await Navigation.PushModalAsync(new AdminPage().WithinNavigationPage());
		//}

		async void OnAboutSelected()
		{
			var page = new AboutPage();
			page.AddDoneButton();

			await Navigation.PushModalAsync(page.WithinNavigationPage());
		}
	}

	public partial class AthleteLeaguesXaml : BaseContentPage<AthleteLeaguesViewModel>
	{
	}
}