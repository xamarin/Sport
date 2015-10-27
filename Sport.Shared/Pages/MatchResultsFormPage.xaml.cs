using System;
using Xamarin.Forms;
using System.Collections.Generic;

namespace Sport.Shared
{
	public partial class MatchResultsFormPage : MatchResultsFormXaml
	{
		public Action OnMatchResultsPosted
		{
			get;
			set;
		}

		public MatchResultsFormPage(Challenge challenge)
		{
			ViewModel.ChallengeId = challenge.Id;
			SetTheme(challenge.League);

			Initialize();
		}

		protected override void Initialize()
		{
			InitializeComponent();
			Title = "Match Score";

			ViewModel.Challenge.MatchResult.Clear();
			for(int i = 0; i < ViewModel.Challenge.League.MatchGameCount; i++)
			{
				var gameResult = new GameResult {
					Index = i,
					ChallengeId = ViewModel.Challenge.Id,
				};

				ViewModel.Challenge.MatchResult.Add(gameResult);

				var form = new GameResultFormView(ViewModel.Challenge, gameResult, i);
				games.Children.Add(form);
			}

			btnSubmit.Clicked += async(sender, e) =>
			{
				var errorMsg = ViewModel.Challenge.ValidateMatchResults();

				if(errorMsg != null)
				{
					errorMsg?.ToToast(ToastNotificationType.Error, "No can do");
					return;
				}

				bool submit = await DisplayAlert("This will end the match", "Are you sure you want to submit these scores?", "Yes", "No");

				if(submit)
				{
					bool success = false;
					using(new HUD("Posting results..."))
					{
						success = await ViewModel.PostMatchResults();
					}

					if(!success)
						return;
					
					await Navigation.PopModalAsync();

					if(OnMatchResultsPosted != null)
						OnMatchResultsPosted();

					"Results submitted - congrats to {0}".Fmt(ViewModel.Challenge.WinningAthlete.Alias).ToToast();
				}
			};
		}

		protected override void TrackPage(Dictionary<string, string> metadata)
		{
			if(ViewModel?.Challenge != null)
				metadata.Add("challengeId", ViewModel.Challenge.Id);

			base.TrackPage(metadata);
		}
	}

	public partial class MatchResultsFormXaml : BaseContentPage<MatchResultFormViewModel>
	{
	}
}