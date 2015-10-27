using System;
using Xamarin.Forms;
using System.Collections.Generic;

namespace Sport.Shared
{
	public partial class ChallengeDatePage : ChallengeDateXaml
	{
		public Action<Challenge> OnChallengeSent
		{
			get;
			set;
		}

		public Athlete Challengee
		{
			get;
			private set;
		}

		public League League
		{
			get;
			private set;
		}

		public ChallengeDatePage(Athlete challengee, League league)
		{
			SetTheme(league);
			ViewModel.CreateChallenge(App.CurrentAthlete, challengee, league);
			Challengee = challengee;
			League = league;

			Initialize();
		}

		protected override void Initialize()
		{
			InitializeComponent();
			Title = "Date and Time";

			btnChallenge.Clicked += async(sender, e) =>
			{
				var errors = ViewModel.Validate();

				if(errors != null)
				{
					errors.ToToast(ToastNotificationType.Error);
					return;
				}

				Challenge challenge;
				using(new HUD("Sending challenge..."))
				{
					challenge = await ViewModel.PostChallenge();
				}

				if(OnChallengeSent != null && challenge != null && challenge.Id != null)
					OnChallengeSent(challenge);
			};

			var btnCancel = new ToolbarItem {
				Text = "Cancel"
			};

			btnCancel.Clicked += async(sender, e) =>
			{
				await Navigation.PopModalAsync();		
			};

			ToolbarItems.Add(btnCancel);
		}

		protected override void OnDisappearing()
		{
			ViewModel.CancelTasks();
			base.OnDisappearing();
		}

		protected override void TrackPage(Dictionary<string, string> metadata)
		{
			if(ViewModel?.Challenge != null)
			{
				metadata.Add("challengeId", ViewModel.Challenge.Id);
				metadata.Add("challengerAthleteId", ViewModel.Challenge.ChallengerAthleteId);
				metadata.Add("challengeeAthleteId", ViewModel.Challenge.ChallengeeAthleteId);
			}

			base.TrackPage(metadata);
		}
	}

	public partial class ChallengeDateXaml : BaseContentPage<ChallengeDateViewModel>
	{
	}
}