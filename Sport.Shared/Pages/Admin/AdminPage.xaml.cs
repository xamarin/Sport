using Xamarin.Forms;


namespace Sport.Shared
{
	public partial class AdminPage : AdminPageXaml
	{
		public AdminPage()
		{
			Initialize();
		}

		protected override void Initialize()
		{
			base.Initialize();

			Title = "Admin";
			InitializeComponent();

			btnLeagues.Clicked += async(sender, e) =>
			{
				await Navigation.PushAsync(new LeagueLandingPage());	
			};

			btnAthletes.Clicked += async(sender, e) =>
			{
				await Navigation.PushAsync(new AthleteListPage());	
			};

			var btnCancel = new ToolbarItem {
				Text = "Cancel",
			};

			btnCancel.Clicked += async(sender, e) =>
			{
				await Navigation.PopModalAsync();		
			};

			ToolbarItems.Add(btnCancel);
		}

		//		public ICommand OrgSettingsCommand
		//		{
		//			get
		//			{
		//				return new Command(() => Navigation.PushAsync(new AthleteListPagePage()));
		//			}
		//		}
	}

	public partial class AdminPageXaml : BaseContentPage<AdminViewModel>
	{
	}
}