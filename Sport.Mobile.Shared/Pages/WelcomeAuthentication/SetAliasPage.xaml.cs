using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sport.Mobile.Shared
{
	public partial class SetAliasPage : SetAliasPageXaml
	{
		public Action OnSave
		{
			get;
			set;
		}

		public SetAliasPage()
		{
			NavigationPage.SetHasNavigationBar(this, false);
			ViewModel.AthleteId = App.Instance.CurrentAthlete.Id;

			Initialize();
		}

		protected override void Initialize()
		{
			InitializeComponent();
			Title = "Athlete Alias";
			profileStack.Opacity = 0;

			btnSave.Clicked += async(sender, e) =>
			{
				if(string.IsNullOrWhiteSpace(ViewModel.Athlete.Alias))
				{
					"Please enter an alias.".ToToast(ToastNotificationType.Warning);
					return;
				}

				bool success;
				success = await ViewModel.SaveAthlete();

				//Will get offline sync conflict errors for all but one device, ignore and proceed if running in XTC
				if(App.Instance.CurrentAthlete.Email.StartsWith("rob.testcloud"))
					success = true;

				if(success)
				{
					if(OnSave != null)
						OnSave();

					await profileStack.LayoutTo(new Rectangle(0, Content.Width * -1, profileStack.Width, profileStack.Height), App.AnimationSpeed, Easing.SinIn);
					await label1.FadeTo(0, App.AnimationSpeed, Easing.SinIn);
					await aliasBox.FadeTo(0, App.AnimationSpeed, Easing.SinIn);
					await label2.FadeTo(0, App.AnimationSpeed, Easing.SinIn);
					await buttonStack.FadeTo(0, App.AnimationSpeed, Easing.SinIn);

					var page = new EnablePushPage();
					page.ViewModel.AthleteId = ViewModel.AthleteId;

					await Navigation.PushAsync(page);
				}
			};
		}

		protected async override void OnLoaded()
		{
			profileStack.Layout(new Rectangle(0, profileStack.Height * -1, profileStack.Width, profileStack.Height));
			base.OnLoaded();

			await Task.Delay(App.DelaySpeed);
			profileStack.Opacity = 1;

			await profileStack.LayoutTo(new Rectangle(0, 0, profileStack.Width, profileStack.Height), App.AnimationSpeed, Easing.SinIn);
			await label1.ScaleTo(1, App.AnimationSpeed, Easing.SinIn);
			await aliasBox.ScaleTo(1, App.AnimationSpeed, Easing.SinIn);
			await label2.ScaleTo(1, App.AnimationSpeed, Easing.SinIn);
			await buttonStack.ScaleTo(1, App.AnimationSpeed, Easing.SinIn);

//			await Task.Delay(1000);
//
//			Device.BeginInvokeOnMainThread(() =>
//			{
//				if(txtAlias != null)
//					txtAlias.Focus();
//			});
		}
	}

	public partial class SetAliasPageXaml : BaseContentPage<AthleteProfileViewModel>
	{
	}
}