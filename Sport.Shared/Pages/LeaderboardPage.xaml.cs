using Xamarin.Forms;
using System;
using System.Collections.Generic;

namespace Sport.Shared
{
	public partial class LeaderboardPage : LeaderboardXaml
	{
		public LeaderboardPage(League league)
		{
			ViewModel.League = league;
			SetTheme(league);

			Initialize();
		}

		~LeaderboardPage()
		{
			MessagingCenter.Unsubscribe<App>(this, Messages.ChallengesUpdated);
		}

		protected override void Initialize()
		{
			InitializeComponent();
			Title = "Leaderboard";

			list.ItemSelected += async(sender, e) =>
			{
				if(list.SelectedItem == null)
					return;

				var vm = list.SelectedItem as MembershipViewModel;
				list.SelectedItem = null;
				var page = new MembershipDetailsPage(vm.Membership.Id);
					
				await Navigation.PushAsync(page);
			};

			if(ViewModel.League != null)
				ViewModel.LocalRefresh();

			SubscribeToChallenges();
		}

		protected override void TrackPage(Dictionary<string, string> metadata)
		{
			if(ViewModel?.League != null)
				metadata.Add("leagueId", ViewModel.League.Id);

			base.TrackPage(metadata);
		}

		void SubscribeToChallenges()
		{
			var self = new WeakReference<LeaderboardPage>(this);
			Action<App> action = (app) =>
			{
				LeaderboardPage v;
				if(!self.TryGetTarget(out v))
					return;

				v.ViewModel.LocalRefresh();
			};
			MessagingCenter.Subscribe<App>(this, Messages.ChallengesUpdated, action);
		}

		protected override void OnAppearing()
		{
			if(ViewModel.League != null)
				ViewModel.LocalRefresh();

			base.OnAppearing();
		}

		protected override async void OnIncomingPayload(NotificationPayload payload)
		{
			base.OnIncomingPayload(payload);

			string leagueId;
			if(payload.Payload.TryGetValue("leagueId", out leagueId))
			{
				if(leagueId == ViewModel.League.Id)
				{
					await ViewModel.GetLeaderboard(true);
				}
			}
		}
	}

	public partial class LeaderboardXaml : BaseContentPage<LeaderboardViewModel>
	{
	}
}