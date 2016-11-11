using System;
using Android.Views;
using Sport.Mobile.Shared;

namespace Sport.Mobile.Droid
{
	public class StatusBarManager : IStatusBarManager
	{
		public StatusBarManager()
		{
		}

		public void SetColor(string color)
		{
		}

		public void SetColor(Xamarin.Forms.Color color)
		{
			//Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
			//Window.SetStatusBarColor(Color.Red);
		}
	}
}
