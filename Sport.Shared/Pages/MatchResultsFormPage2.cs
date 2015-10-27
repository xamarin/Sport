using System;
using Xamarin.Forms;

namespace SportChallengeMatchRank.Shared
{
	public class MatchResultsFormPage : BaseContentPage<MatchResultFormViewModel>
	{
		public Action OnMatchResultsPosted
		{
			get;
			set;
		}

		public MatchResultsFormPage(Challenge challenge)
		{
			BarBackgroundColor = challenge.League.Theme.Light;
			BarTextColor = challenge.League.Theme.Dark;

			ViewModel.ChallengeId = challenge.Id;
			Initialize();
		}

		protected override void Initialize()
		{
			Title = "Match Score";

			var scrollView = new ScrollView {
				BackgroundColor = Color.White,
			};

			var stackLayout = new StackLayout {
				Spacing = 10,
				Padding = 0,
			};

			var btnSubmit = new Button {
				Text = "Post Match Score",
				BackgroundColor = ViewModel.Challenge.League.Theme.Dark,
			};

			var btnCancel = new ToolbarItem {
				Text = "Cancel"
			};
			ToolbarItems.Add(btnCancel);

			btnCancel.Clicked += async(sender, e) =>
			{
				await Navigation.PopModalAsync();		
			};

			ViewModel.Challenge.MatchResult.Clear();
			for(int i = 0; i < ViewModel.Challenge.League.MatchGameCount; i++)
			{
				var gameResult = new GameResult {
					Index = i,
					ChallengeId = ViewModel.Challenge.Id,
				};

				ViewModel.Challenge.MatchResult.Add(gameResult);

				var form = new GameResultFormView(ViewModel.Challenge, gameResult, i);
				stackLayout.Children.Add(form);
			}

			scrollView.Content = stackLayout;
			stackLayout.Children.Add(new StackLayout {
				Padding = 24,
				Children = {
					btnSubmit
				},
			});
			Content = scrollView;

			btnSubmit.Clicked += async(sender, e) =>
			{
				var errorMsg = ViewModel.ValidateMatchResults();

				if(errorMsg != null)
				{
					errorMsg?.ToToast(ToastNotificationType.Error, "No can do");
					return;
				}

				bool submit = await DisplayAlert("This will end the match", "Are you sure you want to submit these scores?", "Yes", "No");

				if(submit)
				{
					using(new HUD("Posting results..."))
					{
						await ViewModel.PostMatchResults();
					}

					await Navigation.PopModalAsync();

					if(OnMatchResultsPosted != null)
						OnMatchResultsPosted();

					var title = App.CurrentAthlete.Id == ViewModel.Challenge.WinningAthlete.Id ? "Victory!" : "Bummer";
					"Results submitted - congrats to {0}!".Fmt(ViewModel.Challenge.WinningAthlete.Name).ToToast(ToastNotificationType.Success, title);
				}
			};
		}
	}
}