using Android.Text;
using Android.Views;
using Sport.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(SportEntry), typeof(Sport.Android.SportEntryRenderer))]
namespace Sport.Android
{
	public class SportEntryRenderer : EntryRenderer
	{

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			var view = (SportEntry)Element;

			SetFont(view);
			SetTextAlignment(view);
			SetMaxLength(view);
		}

		protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			var view = (SportEntry)Element;

			if(e.PropertyName == SportEntry.FontProperty.PropertyName)
				SetFont(view);
			
			if(e.PropertyName == SportEntry.XAlignProperty.PropertyName)
				SetTextAlignment(view);
		}

		void SetTextAlignment(SportEntry view)
		{
			switch(view.XAlign)
			{
				case Xamarin.Forms.TextAlignment.Center:
					Control.Gravity = GravityFlags.CenterHorizontal;
					break;
				case Xamarin.Forms.TextAlignment.End:
					Control.Gravity = GravityFlags.End;
					break;
				case Xamarin.Forms.TextAlignment.Start:
					Control.Gravity = GravityFlags.Start;
					break;
			}
		}

		void SetFont(SportEntry view)
		{
			if(view.Font != Font.Default)
			{
				Control.TextSize = view.Font.ToScaledPixel();
			}
		}

		void SetMaxLength(SportEntry view)
		{
			Control.SetFilters(new IInputFilter[] {
				new InputFilterLengthFilter(view.MaxLength)
			});
		}
	}
}