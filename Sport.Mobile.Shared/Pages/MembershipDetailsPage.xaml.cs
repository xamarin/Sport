using System;
using Xamarin.Forms;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Sport.Mobile.Shared
{
	public partial class MembershipDetailsPage : MembershipDetailsXaml
	{
		public Action OnDelete
		{
			get;
			set;
		}

		public MembershipDetailsPage ()
		{
			Initialize ();
		}

		public MembershipDetailsPage(Membership membership)
		{
			ViewModel.Membership = membership;
			SetTheme(ViewModel.Membership?.League);

			Initialize();
		}

		protected override void Initialize()
		{
			InitializeComponent();
			Title = "Membership Info";
			profileStack.Theme = App.Instance.Theming.GetThemeFromColor("gray");

			btnPast.Clicked += async(sender, e) =>
			{
				var historyPage = new ChallengeHistoryPage(ViewModel.Membership);
				historyPage.AddDoneButton("Done");

				await Navigation.PushModalAsync(historyPage.WithinNavigationPage());
				await Task.Delay(100);
				await historyPage.ViewModel.GetChallengeHistory();
			};

			btnChallenge.Clicked += async(sender, e) =>
			{
				var conflict = ViewModel.Membership.GetChallengeConflictReason(App.Instance.CurrentAthlete);
				if(conflict != null)
				{
					conflict.ToToast();
					return;
				}

				var datePage = new ChallengeDatePage(ViewModel.Membership.Athlete, ViewModel.Membership.League);

				datePage.OnChallengeSent = async(challenge) =>
				{
					ViewModel.NotifyPropertiesChanged();
					await Navigation.PopModalAsync();
					await Navigation.PopAsync();

					"Challenge sent".Fmt(ViewModel.Membership.Athlete.Name).ToToast(ToastNotificationType.Success);
				};

				await Navigation.PushModalAsync(datePage.WithinNavigationPage());
			};

			ViewModel.SetPropertyChanged("CanChallenge");
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			ViewModel.Membership.LocalRefresh();
			ViewModel.NotifyPropertiesChanged();
		}

		protected override void TrackPage(Dictionary<string, string> metadata)
		{
			if(ViewModel?.Membership != null)
				metadata.Add("membershipId", ViewModel.Membership.Id);

			base.TrackPage(metadata);
		}

		protected override async void OnIncomingPayload(NotificationPayload payload)
		{
			base.OnIncomingPayload(payload);

			var reload = false;
			string membershipId = null;
			string winningAthleteId = null;
			string losingAthleteId = null;

			if(payload.Payload.TryGetValue("membershipId", out membershipId) && membershipId == ViewModel.Membership.Id)
				reload = true;

			if(payload.Payload.TryGetValue("winningAthleteId", out winningAthleteId) && payload.Payload.TryGetValue("losingAthleteId", out losingAthleteId))
			{
				reload |= winningAthleteId == ViewModel.Membership.AthleteId || losingAthleteId == ViewModel.Membership.AthleteId;
			}

//			reload |= payload.Payload.TryGetValue("challengeId", out challengeId) && ViewModel.Membership.Athlete.AllChallenges.Any(c => c.Id == challengeId);

			if(reload)
			{
				await ViewModel.RefreshMembership();
			}
		}
	}

	public partial class MembershipDetailsXaml : BaseContentPage<MembershipDetailsViewModel>
	{

	}
}