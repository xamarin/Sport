using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Sport.Shared
{
	public partial class AthleteLeaguesPage : AthleteLeaguesXaml
	{
		public AthleteLeaguesPage(string athleteId = null)
		{
			//ViewModel is newed up in the ViewModel getter of BaseContentPage
			ViewModel.AthleteId = athleteId;
			Initialize();
		}

		protected override void Initialize()
		{
			InitializeComponent();
			Title = "Leagues";

			if(App.CurrentAthlete != null)
				ViewModel.LocalRefresh();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			OnUserAuthenticated();

			btnJoin.Clicked += OnJoinClicked;
			list.ItemSelected += OnListItemSelected;

			foreach(var l in ViewModel.Leagues)
				l.NotifyPropertiesChanged();
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
			page.OnJoinedLeague = (l) =>
			{
				var self = (AthleteLeaguesPage)weakSelf.Target;
				if(self == null)
					return;
				
				self.ViewModel.LocalRefresh();
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
			page.OnAbandondedLeague = (async(l) =>
			{
				var self = (AthleteLeaguesPage)weakSelf.Target;
				if(self == null)
					return;

				self.ViewModel.LocalRefresh();
				self.ViewModel.SetPropertyChanged("Athlete");
				await self.Navigation.PopAsync();
			});

			await Navigation.PushAsync(page);
		}

		internal override void OnUserAuthenticated()
		{
			base.OnUserAuthenticated();

			if(App.CurrentAthlete != null)
			{
				ViewModel.AthleteId = App.CurrentAthlete.Id;
				ViewModel.LocalRefresh();
			}
		}

		protected override async void OnIncomingPayload(NotificationPayload payload)
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
				await ViewModel.RemoteRefresh();
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

//			if(App.CurrentAthlete.IsAdmin)
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

			if(action == _admin)
				OnAdminSelected();

			if(action == _about)
				OnAboutSelected();
		}

		void OnLogoutSelected()
		{
			LogoutUser();
		}

		async void OnProfileSelected()
		{
			var page = new AthleteProfilePage(App.CurrentAthlete.Id);
			page.OnSave = async() => await Navigation.PopModalAsync();
			await Navigation.PushModalAsync(page.WithinNavigationPage());
		}

		async void OnAdminSelected()
		{
			await Navigation.PushModalAsync(new AdminPage().WithinNavigationPage());
		}

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