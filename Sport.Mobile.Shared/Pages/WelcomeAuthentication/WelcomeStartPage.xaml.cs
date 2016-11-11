using System;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Sport.Mobile.Shared
{
	public partial class WelcomeStartPage : WelcomeStartPageXaml
	{
		public WelcomeStartPage()
		{
			NavigationPage.SetHasNavigationBar(this, false);
			Initialize();
		}

		protected override void Initialize()
		{
			InitializeComponent();
			Title = "Welcome!";

			btnAuthenticate.Clicked += async(sender, e) =>
			{
				await ViewModel.Authenticate();

				if(App.Instance.CurrentAthlete != null)
				{
					await label1.FadeTo(0, App.AnimationSpeed, Easing.SinIn);
					await label2.FadeTo(0, App.AnimationSpeed, Easing.SinIn);
					await buttonStack.FadeTo(0, App.AnimationSpeed, Easing.SinIn);
					await Navigation.PushAsync(new SetAliasPage());
				}
			};
		}

		protected async override void OnLoaded()
		{
			base.OnLoaded();

			await Task.Delay(App.DelaySpeed);
			await label1.ScaleTo(1, App.AnimationSpeed, Easing.SinIn);
			await label2.ScaleTo(1, App.AnimationSpeed, Easing.SinIn);
			await buttonStack.ScaleTo(1, App.AnimationSpeed, Easing.SinIn);
		}
	}

	public partial class WelcomeStartPageXaml : BaseContentPage<AuthenticationViewModel>
	{
	}
}