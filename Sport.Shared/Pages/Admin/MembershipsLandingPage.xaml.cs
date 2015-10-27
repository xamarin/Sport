using Xamarin.Forms;

namespace SportChallengeMatchRank.Shared
{
	public partial class MembershipsLandingPage : MembershipsLandingXaml
	{
		bool _hasLoadedBefore;

		public MembershipsLandingPage(Athlete athlete)
		{
			ViewModel.Athlete = athlete;
			Initialize();
		}

		public MembershipsLandingPage(League league)
		{
			ViewModel.League = league;
			Initialize();
		}

		protected override void Initialize()
		{
			InitializeComponent();
			Title = "Memberships";

			list.ItemSelected += async(sender, e) =>
			{
				if(list.SelectedItem == null)
					return;

				var member = list.SelectedItem as Membership;
				list.SelectedItem = null;

				var detailsPage = new MembershipDetailsPage(member.Id);
				detailsPage.OnDelete = () =>
				{
					var athlete = DataManager.Instance.Athletes.Get(member.AthleteId);
					if(athlete != null)
					{
						athlete.Memberships.RemoveModel(member);
					}
								
					var league = DataManager.Instance.Leagues.Get(member.LeagueId);
					if(league != null)
					{
						league.Memberships.RemoveModel(member);
					}
	
					detailsPage.OnDelete = null;
				};

				await Navigation.PushModalAsync(new NavigationPage(detailsPage));
			};
		}

		async protected override void OnLoaded()
		{
			using(new HUD("Getting members..."))
			{
				if(ViewModel.Athlete != null)
				{
					await ViewModel.GetAllMembershipsByAthlete();
					list.RefreshCommand = ViewModel.GetAllMembershipsByAthleteCommand;
					list.SetBinding(ListView.ItemsSourceProperty, "Athlete.Memberships");
					_hasLoadedBefore = true;
				}

				if(ViewModel.League != null)
				{
					await ViewModel.GetLeaderboard();
					list.RefreshCommand = ViewModel.GetLeaderboardCommand;
					list.SetBinding(ListView.ItemsSourceProperty, "League.Memberships");
					_hasLoadedBefore = true;
				}
			}

			base.OnLoaded();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			if(!_hasLoadedBefore)
				return;

			if(ViewModel.League != null)
				ViewModel.League.RefreshMemberships();

			if(ViewModel.Athlete != null)
				ViewModel.Athlete.RefreshMemberships();

			UpdateBindings();
		}

		void UpdateBindings()
		{
			list.ItemsSource = null;
			if(ViewModel.Athlete != null)
			{
				list.SetBinding(ListView.ItemsSourceProperty, "Athlete.Memberships");
			}

			if(ViewModel.League != null)
			{
				list.SetBinding(ListView.ItemsSourceProperty, "League.Memberships");
			}
		}
	}

	public partial class MembershipsLandingXaml : BaseContentPage<LeaderboardViewModel>
	{
	}
}