using System;
using System.Linq;
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
			Authenticate();
			RegisterAthlete();

			app.WaitForElement(e => e.Marked("leagueRow"), "Timed out waiting for league row", TimeSpan.FromMinutes(2));
			app.Screenshot("Now I should see a list of leagues I have joined");

			app.Tap("joinLeagueButton");
			app.WaitForElement(e => e.Marked("leagueRow"));

			if(TestEnvironment.IsTestCloud)
				Thread.Sleep(10000); //Need to wait for list images to load

			app.Screenshot("Then I should see a list of leagues to join");

			Thread.Sleep(1000);
			app.Tap("leagueRow");

			app.ScrollDownTo("leaderboardButton", "leagueDetailsScrollView");
			app.Screenshot("Then I should see a league I can join");

			app.Back(platform);
			app.Tap("Done");

			app.Screenshot("Athlete leagues listview");
			app.ScrollDownTo("Billiards");
			app.Tap("Billiards");

			app.WaitForElement("leaguePhoto");
			app.Screenshot("Then I should see the league details");
			app.ScrollDownTo("leaderboardButton", "leagueDetailsScrollView", ScrollStrategy.Gesture, .67, 1000, true, TimeSpan.FromMinutes(1));
			app.Tap("leaderboardButton");

			app.WaitForElement("memberItemRoot");
			app.Screenshot("Leaderboard listview");

			app.ScrollDownTo("*You*", "leaderboardList", ScrollStrategy.Gesture, .67, 300, true, TimeSpan.FromMinutes(1));

			var results = app.Query("aliasLabel");
			//var previous = results.SingleOrDefault(e => e.Text == "*You*");
			var previous = results.TakeWhile(e => e.Text != "*You*").LastOrDefault();

			//Tap the row previous to the test user
			app.TapCoordinates(previous.Rect.CenterX, previous.Rect.CenterY);

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

			app.WaitForElement("Membership Info");
			app.Back(platform);

			app.WaitForElement("Leaderboard");
			app.Back(platform);

			app.Tap("moreButton");
			app.Tap("Cowardly Abandon League");

			app.Screenshot("Confirm");
			app.Tap("No");

			app.Back(platform);
			app.Screenshot("End");

			app.Tap("moreButton");
			app.Screenshot("More options menu");
			app.Tap(e => e.Marked("About"), "About page");

			app.WaitForElement("aboutPage");
			app.ScrollDownTo("sourceButton");
			app.Screenshot("Bottom of About page");

			app.Tap("Done");

			app.Tap("moreButton");
			app.Tap(e => e.Marked("My Profile"), "Profile page");
			app.ScrollDownTo("saveButton");
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

		void Authenticate()
		{
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

			app.Tap(e => e.Css(passwordId));
			app.EnterText(e => e.Css(passwordId), Keys.TestPassword, "And I enter my super secret password");
			app.DismissKeyboard();
			app.Tap(e => e.Css(nextButtonPasswordId));
		}

		void RegisterAthlete()
		{
			app.WaitForElement(e => e.Marked("aliasText"), "Timed out waiting for aliasText", TimeSpan.FromMinutes(5));
			app.ClearText(e => e.Marked("aliasText"));
			app.EnterText(e => e.Marked("aliasText"), Keys.TestAlias, "And I enter my alias");
			app.DismissKeyboard();
			Thread.Sleep(3000);

			app.Tap("And I save my profile", e => e.Marked("saveButton"));
			app.WaitForElement("continueButton", "Timed out waiting for the Continue button", TimeSpan.FromMinutes(5));
			Thread.Sleep(3000);

			ToggleSwitch("pushToggle", true);
			app.Screenshot("And I enable push notifications");
			ToggleSwitch("pushToggle", false);
			
			app.Tap("Continue button", e => e.Marked("continueButton"));
		}

		void ToggleSwitch(string marked, bool isToggled)
		{
			if(platform == Platform.iOS)
			{
				app.Query(e => e.Marked(marked).Invoke("setOn", Convert.ToInt32(isToggled), "animated", 1));
			}
			else if (platform == Platform.Android)
			{
				app.Query(e => e.Marked(marked).Invoke("setChecked", isToggled));
			}
		}
	}
}