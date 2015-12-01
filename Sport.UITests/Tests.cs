using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.TestCloud.Extensions;
using System.Threading;
using System;
using Xamarin.UITest.Queries;
using System.Linq;

namespace Sport.UITests
{
	[TestFixture(Platform.Android)]
	[TestFixture(Platform.iOS)]
	public class Tests
	{
		IApp app;
		Platform platform;

		public Tests(Platform platform)
		{
			this.platform = platform;
		}

		[SetUp]
		public void BeforeEachTest()
		{
			app = AppInitializer.StartApp(platform);
		}

		[Test]
		public void JoinLeagueAndChallenge()
		{
			Func<AppQuery, AppQuery> menuButton = e => e.Marked("ic_more_vert_white");
			if(platform == Platform.Android)
				menuButton = e => e.Marked("NoResourceEntry-0").Index(app.Query(ee => ee.Marked("NoResourceEntry-0")).Length - 1);

			app.WaitForElement("authButton");
			app.Tap("When the app starts", "authButton");

			app.WaitForElement(e => e.Css("#Email"));
			app.EnterText(e => e.Css("#Email"), Keys.TestEmail, "And I enter my email address");
			app.DismissKeyboard();

			if(app.Query(e => e.Css("#next")).Length > 0)
			{
				Thread.Sleep(2000); //Can't wait for element because it will show but is disabled
				app.Tap(e => e.Css("#next"));
			}

			if(TestEnvironment.IsTestCloud)
				Thread.Sleep(10000); //Need to wait for form fields to animate over

			app.Tap(e => e.Css("#Passwd"));
			app.EnterText(e => e.Css("#Passwd"), Keys.TestPassword, "And I enter my super secret password");
			app.DismissKeyboard();

			app.Tap("And I click the Sign In button", e => e.Css("#signIn"));

			Thread.Sleep(2000); //Can't wait here because the dialog is conditional
			if(app.Query(e => e.Button("Remember")).Length > 0)
				app.Back();

			Thread.Sleep(5000);
			if(app.Query(e => e.Css("#grant_heading")).Length > 0)
			{
				app.ScrollDownTo(e => e.Css("#submit_approve_access"));
				app.Tap("And I accept the terms", e => e.Css("#submit_approve_access"));
			}

			app.WaitForElement(e => e.Marked("aliasText"));
			app.ClearText(e => e.Marked("aliasText"));
			app.EnterText(e => e.Marked("aliasText"), "XTC Tester", "And I enter my alias");
			app.DismissKeyboard();

			app.Tap("And I save my profile", e => e.Marked("saveButton"));

			Thread.Sleep(3000);
			app.WaitForElement("continueButton");
			app.Tap("Continue button", e => e.Marked("continueButton"));

			app.WaitForElement(e => e.Marked("leagueRow"));
			app.Screenshot("Now I should see a list of leagues I have joined");

			//Available leagues
//			Helpers.OnAndroid("NoResourceEntry-0".Tap);
//			Helpers.OniOS("ic_add_white".Tap);

			if(platform == Platform.Android)
				app.Tap("NoResourceEntry-0");
			else if(platform == Platform.iOS)
				app.Tap("ic_add_white");

			//Thread.Sleep(5000);
			app.WaitForElement(e => e.Marked("leagueRow"));

			if(TestEnvironment.IsTestCloud)
				Thread.Sleep(10000); //Need to wait for list images to load
			
			app.Screenshot("Then I should see a list of leagues to join");

			Thread.Sleep(1000);
			app.Tap("leagueRow");

			app.WaitForElement("leaderboardButton");
			app.Screenshot("Then I should see a league I can join");

			app.Back(platform);
			app.Tap("Done");

			app.Screenshot("Athlete leagues listview");
			app.ScrollDownTo("Coin Flip");
			app.Tap("Coin Flip");

			app.WaitForElement("leaguePhoto");
			app.Screenshot("Then I should see the league details");
			app.ScrollDownTo("leaderboardButton");
			app.Tap("leaderboardButton");

			app.WaitForElement("memberItemRoot");
			app.Screenshot("Leaderboard listview");

			app.ScrollDownTo(e => e.Text ("Rob TestCloud"));

			Thread.Sleep(1000); //Let scrolling settle

			var result = app.Query(e => e.Text("Rob TestCloud"))[0];
			app.TapCoordinates(result.Rect.X, result.Rect.Y - 100); //Select player above self
			app.WaitForElement("memberDetailsRoot");
			app.Screenshot("Member details page");

			app.ScrollDownTo("pastButton");
			app.Tap("Bottom of member details page", e => e.Marked("pastButton"));

			app.WaitForElement("challengeItemRoot");
			app.Screenshot("Challenge history page");

			if(TestEnvironment.IsTestCloud)
				Thread.Sleep(10000); //Need to wait for list to load

			if(app.Query("resultItemRoot").Length > 0)
			{
				app.Tap("resultItemRoot");
				app.WaitForElement("challengeRoot");
				app.Screenshot("Challenge result page");

				app.ScrollDownTo("winningLabel");
				app.Screenshot("Challenge result page bottom");
				app.Back(platform);
				app.Tap("Done");
			}
			else
			{
				app.Tap("Done");
			}

			app.Tap("challengeButton");
			app.Screenshot("Challenge date page");

			app.Tap("datePicker");
			app.Screenshot("Challenge date picker");

			DismissPicker();
			app.Screenshot("End");

			app.Tap("timePicker");
			app.Screenshot("Challenge time picker");

			DismissPicker();
			app.Tap("Cancel");

			app.Back(platform);
			app.Back(platform);

			app.Tap("challengeButton");
			app.Screenshot("Challenge date page");

			app.Tap("datePicker");
			app.Screenshot("Challenge date picker");
			DismissPicker();
			app.Screenshot("End");

			app.Tap("timePicker");
			app.Screenshot("Challenge time picker");
			DismissPicker();
			app.Tap("Cancel");

			app.Screenshot("Back");

			app.Tap(menuButton);
			app.Tap("Cowardly Abandon League");

			app.Screenshot("Confirm");
			app.Tap("No");

			app.Back(platform);
			app.Screenshot("End");

			app.Tap(menuButton);
			app.Screenshot("More options menu");
			app.Tap(e => e.Marked("About"), "About page");

			app.WaitForElement("aboutPage");
			app.ScrollDownTo("sourceButton", "aboutScroll");
			app.Screenshot("Bottom of About page");

			app.Tap("Done");

			app.Tap(menuButton);
			app.Tap(e => e.Marked("My Profile"), "Profile page");
			app.ScrollTo("saveButton");
			app.Tap("Saving profile", e => e.Marked("saveButton"));

			app.WaitForElement(e => e.Marked("leagueRow"));
			app.Screenshot("End of test");
		}

		void DismissPicker()
		{
			if(platform == Platform.Android)
				app.Back();
			else
				app.Tap("Done");
			
//			Helpers.OnAndroid(app.Back);
//			Helpers.OniOS("Done".Tap);
		}
	}
}

