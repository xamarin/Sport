using System.Runtime.CompilerServices;
using Xamarin.Forms;

[assembly: 
	InternalsVisibleTo("Sport.Android"),
	InternalsVisibleTo("Sport.iOS")]

namespace Sport.Shared
{
	public class SportEntry : Entry
	{
		public static readonly BindableProperty HasBorderProperty =
			BindableProperty.Create("HasBorder", typeof(bool), typeof(SportEntry), true);

		public bool HasBorder
		{
			get
			{
				return (bool)GetValue(HasBorderProperty);
			}
			set
			{
				SetValue(HasBorderProperty, value);
			}
		}

		public static readonly BindableProperty FontProperty =
			BindableProperty.Create("Font", typeof(Font), typeof(SportEntry), new Font());

		public Font Font
		{
			get
			{
				return (Font)GetValue(FontProperty);
			}
			set
			{
				SetValue(FontProperty, value);
			}
		}

		public static readonly BindableProperty FontFamilyProperty =
			BindableProperty.Create("FontFamily", typeof(string), typeof(SportEntry), null);

		public string FontFamily
		{
			get
			{
				return (string)this.GetValue(FontFamilyProperty);
			}
			set
			{
				this.SetValue(FontFamilyProperty, value);
			}
		}

		public static readonly BindableProperty MaxLengthProperty =
			BindableProperty.Create("MaxLength", typeof(int), typeof(SportEntry), int.MaxValue);

		public int MaxLength
		{
			get
			{
				return (int)this.GetValue(MaxLengthProperty);
			}
			set
			{
				this.SetValue(MaxLengthProperty, value);
			}
		}

		public static readonly BindableProperty XAlignProperty =
			BindableProperty.Create("XAlign", typeof(TextAlignment), typeof(SportEntry), TextAlignment.Start);

		public TextAlignment XAlign
		{
			get
			{
				return (TextAlignment)GetValue(XAlignProperty);
			}
			set
			{
				SetValue(XAlignProperty, value);
			}
		}
	}
}