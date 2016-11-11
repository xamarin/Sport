using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Linq;

namespace Sport.Mobile.Shared
{
	public partial class ProfileStripView : ContentView
	{
		public ProfileStripView()
		{
			InitializeComponent();
			root.BindingContext = this;
		}

		public static readonly BindableProperty AthleteProperty =
			BindableProperty.Create("Athlete", typeof(Athlete), typeof(ProfileStripView), null);

		public Athlete Athlete
		{
			get
			{
				return (Athlete)GetValue(AthleteProperty);
			}
			set
			{
				SetValue(AthleteProperty, value);
			}
		}

		public static readonly BindableProperty TextColorProperty =
			BindableProperty.Create("TextColor", typeof(Color), typeof(ProfileStripView), Color.White);

		public Color TextColor
		{
			get
			{
				return (Color)GetValue(TextColorProperty);
			}
			set
			{
				SetValue(TextColorProperty, value);
			}
		}

		public static readonly BindableProperty ThemeProperty =
			BindableProperty.Create("Theme", typeof(ColorTheme), typeof(ProfileStripView), null);

		public ColorTheme Theme
		{
			get
			{
				return (ColorTheme)GetValue(ThemeProperty);
			}
			set
			{
				SetValue(ThemeProperty, value);
			}
		}
	}
}