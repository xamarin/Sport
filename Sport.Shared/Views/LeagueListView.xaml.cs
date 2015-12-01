using System;
using Xamarin.Forms;
using System.Globalization;

namespace Sport.Shared
{
	public partial class LeagueListView : ListView
	{
		public LeagueListView() : base(ListViewCachingStrategy.RecycleElement)
		{
			InitializeComponent();
		}
	}

	public class LeaguePaddingValueConverter : IValueConverter
	{
		public static LeaguePaddingValueConverter Instance = new LeaguePaddingValueConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var isLast = (bool)value;
			return isLast ? new Thickness(14) : new Thickness(14, 14, 14, 0);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}