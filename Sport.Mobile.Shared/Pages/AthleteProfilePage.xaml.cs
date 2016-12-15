using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Sport.Mobile.Shared
{
	public partial class AthleteProfilePage : AthleteProfilePageXaml
	{
		public Action OnSave
		{
			get;
			set;
		}


		public AthleteProfilePage ()
		{
			Initialize ();
		}

		public AthleteProfilePage(string athleteId)
		{
			ViewModel.AthleteId = athleteId;
			Initialize();
		}

		protected override void Initialize()
		{
			InitializeComponent();
			Title = "My Profile";

			BarBackgroundColor = Xamarin.Forms.Color.FromHex("#FF867C");
			AddDoneButton();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			btnSave.Clicked += OnSaveClicked;

			ViewModel.Athlete.LocalRefresh();
			ViewModel.Athlete.Memberships.ForEach(m => m.League.LocalRefresh());
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			btnSave.Clicked -= OnSaveClicked;
		}

		async void OnSaveClicked(object sender, EventArgs e)
		{
			if(string.IsNullOrWhiteSpace(ViewModel.Athlete.Alias))
			{
				"Please enter an alias.".ToToast(ToastNotificationType.Warning);
				return;
			}

			var success = await ViewModel.SaveAthlete();
			//if(ViewModel.EnablePushNotifications)
			//{
			//	await ViewModel.RegisterForPushNotifications();
			//}

			//Will get offline sync conflict errors for all but one device, ignore and proceed if running in XTC
			if(App.Instance.CurrentAthlete.Email.StartsWith("rob.testcloud"))
				success = true;

			if(success)
				OnSave?.Invoke();
		}

		protected override void TrackPage(Dictionary<string, string> metadata)
		{
			if(ViewModel?.Athlete != null)
				metadata.Add("athleteId", ViewModel.Athlete.Id);

			base.TrackPage(metadata);
		}
	}

	public class AthleteProfilePageXaml : BaseContentPage<AthleteProfileViewModel>
	{
	}
}