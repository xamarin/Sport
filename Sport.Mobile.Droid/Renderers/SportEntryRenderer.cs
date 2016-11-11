using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Sport.Mobile.Shared;

[assembly: ExportRenderer(typeof(SportEntry), typeof(Sport.Mobile.Droid.SportEntryRenderer))]

namespace Sport.Mobile.Droid
{
	public class SportEntryRenderer : EntryRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			if(Control != null)
			{
				Control.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
			}
		}
	}
}