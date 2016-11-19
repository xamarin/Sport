using System;
using System.Threading;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Sport.Mobile.Tests
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

			var emailId = "#Email";
			var passwordId = "#Passwd";
			var nextButtonEmailId = "#next";
			var nextButtonPasswordId = "#signIn";

			//Give the Google auth form time to load
			Thread.Sleep(15000);

			var query = app.Query(e => e.Css("#identifierId"));

			if(query.Length > 0)
			{
				emailId = "#identifierId";
				passwordId = "#password";
				nextButtonEmailId = "#identifierNext";
				nextButtonPasswordId = "#passwordNext";
			}

			app.WaitForElement(e => e.Css(emailId));
			app.EnterText(e => e.Css(emailId), Keys.TestEmail, "And I enter my email address");
			app.DismissKeyboard();

			if(app.Query(e => e.Css(nextButtonEmailId)).Length > 0)
			{
				app.Tap(e => e.Css(nextButtonEmailId));
			}

			if(TestEnvironment.IsTestCloud)
				Thread.Sleep(10000); //Need to wait for form fields to animate over

			//app.Repl();
			app.Tap(e => e.Css(passwordId));
			app.EnterText(e => e.Css(passwordId), Keys.TestPassword, "And I enter my super secret password");
			app.DismissKeyboard();
			app.Tap(e => e.Css(nextButtonPasswordId));

			//app.Tap("And I click the Sign In button", e => e.Css("#signIn"));

			//Thread.Sleep(2000); //Can't wait here because the dialog is conditional
			//if(app.Query(e => e.Button("Remember")).Length > 0)
			//	app.Back();

			//Thread.Sleep(5000);
			//if(app.Query(e => e.Css("#grant_heading")).Length > 0)
			//{
			//	app.ScrollDownTo(e => e.Css("#submit_approve_access"));
			//	app.Tap("And I accept the terms", e => e.Css("#submit_approve_access"));
			//}

			app.WaitForElement(e => e.Marked("aliasText"), "Timed out waiting for aliasText", TimeSpan.FromMinutes(5));
			app.ClearText(e => e.Marked("aliasText"));
			app.EnterText(e => e.Marked("aliasText"), Keys.TestAlias, "And I enter my alias");
			app.DismissKeyboard();
			Thread.Sleep(3000);
			
			app.Tap("And I save my profile", e => e.Marked("saveButton"));
			app.WaitForElement("continueButton", "Timed out waiting for the Continue button", TimeSpan.FromMinutes(5));
			Thread.Sleep(3000);
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
			app.ScrollDownTo("Billiards");
			app.Tap("Billiards");

			app.WaitForElement("leaguePhoto");
			app.Screenshot("Then I should see the league details");
			app.ScrollDownTo("leaderboardButton", "scrollView");
			app.Tap("leaderboardButton");

			app.WaitForElement("memberItemRoot");
			app.Screenshot("Leaderboard listview");

			app.ScrollDownTo("10");
			app.Tap("10");
			app.WaitForElement("memberDetailsRoot");
			app.Screenshot("Member details page");

			app.ScrollDownTo("pastButton", "scrollView");
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

				app.ScrollDownTo("winningLabel", "scrollView");
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

			app.WaitForElement("Membership Info");
			app.Back(platform);

			app.WaitForElement("Leaderboard");
			app.Back(platform);

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
			app.ScrollDownTo("sourceButton", "scrollView");
			app.Screenshot("Bottom of About page");

			app.Tap("Done");

			app.Tap(menuButton);
			app.Tap(e => e.Marked("My Profile"), "Profile page");
			app.ScrollDownTo("saveButton", "scrollView");
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