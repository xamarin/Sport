using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Linq;

namespace Sport.Shared
{
	public partial class ProfileStripView : ContentView
	{
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

		public ProfileStripView()
		{
			InitializeComponent();
			root.BindingContext = this;
		}
	}
}