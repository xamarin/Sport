using System;
using Xamarin.Forms;
using System.Collections.Generic;

namespace Sport.Shared
{
	public partial class ChallengeDetailsPage : ChallengeDetailsXaml
	{
		#region Properties

		ToolbarItem _moreButton;

		public Action OnDecline
		{
			get;
			set;
		}

		public Action OnAccept
		{
			get;
			set;
		}

		public Action OnPostResults
		{
			get;
			set;
		}

		#endregion

		Challenge _challenge;

		public ChallengeDetailsPage(Challenge challenge)
		{
			_challenge = challenge;
			ViewModel.Challenge = _challenge;
			SetTheme(challenge.League);

			Initialize();
		}

		protected override void OnAppearing()
		{
			RefreshMenuButtons();
			base.OnAppearing();
		}

		async protected override void Initialize()
		{
			InitializeComponent();
			Title = "Challenge";

			_moreButton = new ToolbarItem("More", "ic_more_vert_white", OnMoreClicked);

			list.ItemSelected += (sender, e) =>
			{
				list.SelectedItem = null;
			};

			await ViewModel.GetMatchResults();
			var count = ViewModel.Challenge.League.MatchGameCount;

			if(ViewModel.Challenge.MatchResult != null && ViewModel.Challenge.MatchResult.Count > 0)
				count = ViewModel.Challenge.MatchResult.Count;

			list.HeightRequest = list.RowHeight * count + 50;
		}

		protected override void TrackPage(Dictionary<string, string> metadata)
		{
			if(ViewModel?.Challenge != null)
				metadata.Add("challengeId", ViewModel.Challenge.Id);
		
			base.TrackPage(metadata);
		}

		void RefreshMenuButtons()
		{
			if(GetMoreMenuOptions().Count > 0)
			{
				if(!ToolbarItems.Contains(_moreButton))
					ToolbarItems.Add(_moreButton);
			}
			else
			{
				ToolbarItems.Remove(_moreButton);
			}
		}

		protected override async void OnIncomingPayload(NotificationPayload payload)
		{
			string challengeId;
			string winnerId;
			if(payload.Payload.TryGetValue("challengeId", out challengeId))
			{
				if(challengeId == ViewModel?.Challenge?.Id)
				{
					await ViewModel.RefreshChallenge();
		
					if(ViewModel.Challenge == null)
					{
						OnDecline?.Invoke();
						await Navigation.PopAsync();
						return;
					}
		
					if(payload.Payload.TryGetValue("winningAthleteId", out winnerId))
					{
						OnPostResults?.Invoke();
					}
				}
			}
		}

		async void OnPostChallengeResults()
		{
			var page = new MatchResultsFormPage(ViewModel.Challenge);
			page.OnMatchResultsPosted = () =>
			{
				ViewModel.NotifyPropertiesChanged();
				OnPostResults?.Invoke();
			};
		
			page.AddDoneButton("Cancel");
			await Navigation.PushModalAsync(page.WithinNavigationPage());
		}

		async void OnRevokeChallenge()
		{
			var decline = await DisplayAlert("Really?", "Are you sure you want to revoke challenge?", "Yes", "No");
		
			if(!decline)
				return;
		
			bool success;
			using(new HUD("Revoking challenge..."))
			{
				success = await ViewModel.DeclineChallenge();
			}
		
			if(success)
				"Challenge revoked".ToToast();
		
			OnDecline?.Invoke();
			await Navigation.PopAsync();
		}

		async void OnAcceptChallenge()
		{
			bool success;
			using(new HUD("Accepting challenge..."))
			{
				success = await ViewModel.AcceptChallenge();
			}
		
			if(success)
				"Challenge accepted".ToToast(ToastNotificationType.Success);
		
			if(OnAccept != null)
				OnAccept();
		}

		async void OnDeclineChallenge()
		{
			var decline = await DisplayAlert("Really?", "Are you sure you want to cowardly decline this honorable duel?", "Yes", "No");
		
			if(!decline)
				return;
		
			bool success;
			using(new HUD("Declining..."))
			{
				success = await ViewModel.DeclineChallenge();
			}
		
			if(success)
				"Challenge declined".ToToast();
		
			OnDecline?.Invoke();
			await Navigation.PopAsync();
		}

		async void OnNudgeAthlete()
		{
			using(new HUD("Nudging..."))
			{
				await ViewModel.NudgeAthlete();
			}
		
			"{0} has been nudged".Fmt(ViewModel.Opponent.Alias).ToToast();
		}

		const string _accept = "Accept Challenge";
		const string _revoke = "Revoke Challenge";
		const string _decline = "Decline Challenge";
		const string _post = "Post Match Results";

		List<string> GetMoreMenuOptions()
		{
			var lst = new List<string>();
		
			if(ViewModel.CanRevoke)
				lst.Add(_revoke);
		
			if(ViewModel.CanDecline || ViewModel.CanDeclineAfterAccept)
				lst.Add(_decline);
		
			return lst;
		}

		async void OnMoreClicked()
		{
			var lst = GetMoreMenuOptions();
			var action = await DisplayActionSheet("Additional actions", "Cancel", null, lst.ToArray());
		
			if(action == _post)
				OnPostChallengeResults();
		
			if(action == _accept)
				OnAcceptChallenge();
		
			if(action == _revoke)
				OnRevokeChallenge();
		
			if(action == _decline)
				OnDeclineChallenge();
		}

		void HandleDeclined(object sender, EventArgs e)
		{
			OnRevokeChallenge();
		}

		void HandleAccepted(object sender, EventArgs e)
		{
			OnAcceptChallenge();
		}

		void HandlePostResults(object sender, EventArgs e)
		{
			OnPostChallengeResults();
		}

		void HandleNudgeAthlete(object sender, EventArgs e)
		{
			OnNudgeAthlete();
		}
	}

	public partial class ChallengeDetailsXaml : BaseContentPage<ChallengeDetailsViewModel>
	{
	}
}