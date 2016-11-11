using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sport.Mobile.Shared
{
	public partial class EnablePushPage : EnablePushPageXaml
	{
		//Flag to disable click that will allow the button text to be updated
		bool _ignoreClicks;

		public Action OnSave
		{
			get;
			set;
		}

		public EnablePushPage()
		{
			NavigationPage.SetHasNavigationBar(this, false);
			Initialize();
		}

		protected override void Initialize()
		{
			InitializeComponent();
			Title = "Enable Push";
			profileStack.Opacity = 0;

			btnCont.Clicked += async(sender, e) =>
			{
				if(_ignoreClicks)
					return;

				_ignoreClicks = true;

				if(ViewModel.EnablePushNotifications)
				{
					var success = await ViewModel.RegisterForPushNotifications();
					if(success)
					{
						btnCont.Text = "THANKS! WE'LL BE IN TOUCH!";
						await AnimateToMainPage();
					}
					else
					{
						_ignoreClicks = false;
						"Unable to register for push notifications".ToToast();
					}
				}
				else
				{
					await AnimateToMainPage();
				}
			};
		}

		protected async override void OnLoaded()
		{
			base.OnLoaded();

			profileStack.Layout(new Rectangle(0, profileStack.Height * -1, profileStack.Width, profileStack.Height));
			profileStack.Opacity = 1;

			await Task.Delay(App.DelaySpeed);
			await profileStack.LayoutTo(new Rectangle(0, 0, profileStack.Width, profileStack.Height), App.AnimationSpeed, Easing.SinIn);

			await Task.Delay(App.DelaySpeed);
			centerStack.ScaleTo(1, App.AnimationSpeed, Easing.SinIn);
			await centerStack.FadeTo(1, App.AnimationSpeed, Easing.SinIn);
			await buttonStack.ScaleTo(1, App.AnimationSpeed, Easing.SinIn);
		}

		async Task AnimateToMainPage()
		{
			var speed = App.AnimationSpeed;
			var ease = Easing.SinIn;

			await profileStack.ScaleTo(0, App.AnimationSpeed, Easing.SinIn);

			//Animations are intentionally not await to allow or concurrent animations
			centerStack.FadeTo(0, speed, ease);
			await centerStack.ScaleTo(0, speed, ease);

			await buttonStack.ScaleTo(0, speed, ease);
			await MoveToMainPage();
		}

		async Task MoveToMainPage()
		{
			Settings.RegistrationComplete = true;
			var page = new AthleteLeaguesPage(App.Instance.CurrentAthlete);
			await page.LoadLeagues();

			await Task.Delay(200);
			await Navigation.PushAsync(page);

			while(Navigation.NavigationStack.Count > 0)
				Navigation.RemovePage(Navigation.NavigationStack[0]); //WelcomeStartPage
		}
	}

	public partial class EnablePushPageXaml : BaseContentPage<AthleteProfileViewModel>
	{
	}
}