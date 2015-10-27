using System;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Sport.Shared
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
			BarBackgroundColor = (Color)App.Current.Resources["bluePrimary"];
			BarTextColor = Color.White;
			BackgroundColor = BarBackgroundColor;
			Title = "Welcome!";

			btnAuthenticate.Clicked += async(sender, e) =>
			{
				btnAuthenticate.IsEnabled = false;
				await ViewModel.AuthenticateCompletely();

				if(App.CurrentAthlete != null)
				{
					await label1.FadeTo(0, (uint)App.AnimationSpeed, Easing.SinIn);
					await label2.FadeTo(0, (uint)App.AnimationSpeed, Easing.SinIn);
					await buttonStack.FadeTo(0, (uint)App.AnimationSpeed, Easing.SinIn);
					await Navigation.PushAsync(new SetAliasPage());
				}
				else
				{
					btnAuthenticate.IsEnabled = true;
				}
			};
		}

		protected async override void OnLoaded()
		{
			base.OnLoaded();

			await Task.Delay(300);
			await label1.ScaleTo(1, (uint)App.AnimationSpeed, Easing.SinIn);
			await label2.ScaleTo(1, (uint)App.AnimationSpeed, Easing.SinIn);
			await buttonStack.ScaleTo(1, (uint)App.AnimationSpeed, Easing.SinIn);
		}
	}

	public partial class WelcomeStartPageXaml : BaseContentPage<AuthenticationViewModel>
	{
	}
}