using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Sport.Shared
{
	public partial class AthleteProfilePage : AthleteProfilePageXaml
	{
		public Action OnSave
		{
			get;
			set;
		}

		public AthleteProfilePage(string athleteId)
		{
			ViewModel.AthleteId = athleteId;
			Initialize();
		}

		protected override void Initialize()
		{
			InitializeComponent();
			Title = "Profile";

			var theme = App.Current.GetThemeFromColor("asphalt");
			profileStack.Theme = theme;

			AddDoneButton();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			btnSave.Clicked += OnSaveClicked;
			btnRegister.Clicked += OnRegisterClicked;

			ViewModel.Athlete.LocalRefresh();
			ViewModel.Athlete.Memberships.ForEach(m => m.League.RefreshChallenges());
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			btnSave.Clicked -= OnSaveClicked;
			btnRegister.Clicked -= OnRegisterClicked;
		}

		void OnRegisterClicked(object sender, EventArgs e)
		{
			ViewModel.RegisterForPushNotifications(async() =>
			{
			});
		}

		async void OnSaveClicked(object sender, EventArgs e)
		{
			if(string.IsNullOrWhiteSpace(ViewModel.Athlete.Alias))
			{
				"Please enter an alias.".ToToast(ToastNotificationType.Warning);
				return;
			}

			bool success;
			using(new HUD("Saving..."))
			{
				success = await ViewModel.SaveAthlete();
			}

			if(success)
			{
				"Your profile has been saved".ToToast();

				if(OnSave != null)
					OnSave();
			}
		}

		protected override void TrackPage(Dictionary<string, string> metadata)
		{
			if(ViewModel?.Athlete != null)
				metadata.Add("athleteId", ViewModel.Athlete.Id);

			base.TrackPage(metadata);
		}
	}

	public partial class AthleteProfilePageXaml : BaseContentPage<AthleteProfileViewModel>
	{
	}
}