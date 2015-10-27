using System;
using Xamarin.Forms;

namespace SportChallengeMatchRank.Shared
{
	public class AthleteTabbedPage : TabbedPage
	{
		public static NavigationPage LeaguesNav
		{
			get;
			private set;
		}

		public static NavigationPage ChallengeNav
		{
			get;
			private set;
		}

		public AthleteTabbedPage()
		{
			LeaguesNav = new NavigationPage(new AthleteLeaguesPage(Settings.Instance.AthleteId)) {
				Title = "My Leagues",
				Icon = new FileImageSource {
					File = "tennis.png",
				}
			};

			ChallengeNav = new NavigationPage(new AthleteChallengesPage(Settings.Instance.AthleteId)) {
				Title = "My Challenges",
				Icon = new FileImageSource {
					File = "fencing.png",
				}
			};

//			LeaguesNav.BarTextColor = Color.White;
//			LeaguesNav.BarBackgroundColor = Color.FromHex("#2c3e5");
//
//			ChallengeNav.BarTextColor = Color.White;
//			ChallengeNav.BarBackgroundColor = Color.FromHex("#2c3e50");

			Children.Add(LeaguesNav);
			Children.Add(ChallengeNav);
		}
	}
}