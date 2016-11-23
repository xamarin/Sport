using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace Sport.Mobile.Droid
{
	[Activity(MainLauncher = true, NoHistory = true, Label = "Sport", Icon = "@drawable/icon", Theme = "@style/LaunchTheme",
	         ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class LaunchActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
		}

		protected override void OnResume()
		{
			base.OnResume();
			StartActivity(typeof(MainActivity));
		}
	}
}
