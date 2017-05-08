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

			BarBackgroundColor = Xamarin.Forms.Color.FromHex("#512DA8");
			AddDoneButton();

			btnSave.Clicked += OnSaveClicked;
			btnSend.Clicked += OnSendPushClicked;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			if (ViewModel?.Athlete != null)
			{
				ViewModel.Athlete.LocalRefresh();
				ViewModel.Athlete.Memberships.ForEach(m => m.League.LocalRefresh());

				if (!ViewModel.EnablePushNotifications)
					buttonStack.Children.Remove(btnSend);
			}
		}

		async void OnSendPushClicked(object sender, EventArgs e)
		{
			await ViewModel.RegisterForPushNotifications();
		}

		async void OnSaveClicked(object sender, EventArgs e)
		{
			if(string.IsNullOrWhiteSpace(ViewModel.Athlete.Alias))
			{
				"Please enter an alias.".ToToast(ToastNotificationType.Warning);
				return;
			}

			var success = await ViewModel.SaveAthlete();

			if (success)
				OnSave?.Invoke();
		}
	}

	public class AthleteProfilePageXaml : BaseContentPage<AthleteProfileViewModel>
	{
	}
}