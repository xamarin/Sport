using System.Threading.Tasks;
using Sport.Mobile.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ThemedNavigationPage), typeof(Sport.Mobile.Droid.ThemedNavigationRenderer))]

namespace Sport.Mobile.Droid
{
	/// <summary>
	/// This custom NavigationRender is only necessary on iOS so we can change the navigation bar color prior to navigating instead of after
	/// Forms currently doesn't give us a lifecycle event before the navigation takes place
	/// This isn't an issue on Android
	/// </summary>
	public class ThemedNavigationRenderer : Xamarin.Forms.Platform.Android.AppCompat.NavigationPageRenderer
	{
		public ThemedNavigationRenderer()
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<NavigationPage> e)
		{
			base.OnElementChanged(e);
			var page = e.NewElement as ThemedNavigationPage;
			ChangeTheme(page);
		}

		void ChangeTheme(ThemedNavigationPage page)
		{
			if(page != null)
			{
				var activity = Forms.Context as MainActivity;
				activity.SetStatusBarColor(page.BarBackgroundColor.ToAndroid());
			}
		}
	}
}