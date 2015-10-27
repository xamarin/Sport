using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sport.Shared
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

			var theme = App.Current.GetThemeFromColor("purple");
			BackgroundColor = theme.Primary;
			profileStack.Opacity = 0;
			profileStack.Theme = theme;

			btnPush.Clicked += (sender, e) =>
			{
				if(_ignoreClicks)
					return;

				_ignoreClicks = true;
				ViewModel.RegisterForPushNotifications(RegisteredForPushNotificationSuccess);
			};

			btnCont.Clicked += async(sender, e) =>
			{
				if(_ignoreClicks)
					return;
				
				_ignoreClicks = true;
				await AnimateToMainPage();
			};
		}

		protected async override void OnLoaded()
		{
			profileStack.Layout(new Rectangle(0, profileStack.Height * -1, profileStack.Width, profileStack.Height));
			base.OnLoaded();

			profileStack.Opacity = 1;
			await Task.Delay(300);
			await profileStack.LayoutTo(new Rectangle(0, 0, profileStack.Width, profileStack.Height), (uint)App.AnimationSpeed, Easing.SinIn);
			await label1.ScaleTo(1, (uint)App.AnimationSpeed, Easing.SinIn);
			await buttonStack.ScaleTo(1, (uint)App.AnimationSpeed, Easing.SinIn);
		}

		void RegisteredForPushNotificationSuccess()
		{
			Device.BeginInvokeOnMainThread(async() =>
			{
				if(App.CurrentAthlete.DeviceToken != null)
				{
					btnPush.Text = "Thanks! We'll be in touch";
					await AnimateToMainPage();
				}
				else
				{
					_ignoreClicks = false;
					"Unable to register for push notifications".ToToast();
				}
			});
		}

		async Task AnimateToMainPage()
		{
			await Task.Delay(App.AnimationSpeed);
			await label1.FadeTo(0, (uint)App.AnimationSpeed, Easing.SinIn);

			var speed = (uint)App.AnimationSpeed;
			var ease = Easing.SinIn;

			var pushRect = new Rectangle(Content.Width, btnPush.Bounds.Y, btnPush.Bounds.Width, btnPush.Height);
			btnPush.FadeTo(0, speed, ease);
			await btnPush.LayoutTo(pushRect, speed, ease);

			var continueRect = new Rectangle(Content.Width, btnCont.Bounds.Y, btnCont.Bounds.Width, btnCont.Height);
			btnCont.FadeTo(0, speed, ease);
			await btnCont.LayoutTo(continueRect, speed, ease);

			MoveToMainPage();
		}

		async void MoveToMainPage()
		{
			Settings.Instance.RegistrationComplete = true;
			await Settings.Instance.Save();

			var page = new AthleteLeaguesPage(App.CurrentAthlete.Id);

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