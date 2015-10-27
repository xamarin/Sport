using Xamarin.Forms;

namespace Sport.Shared
{
	public partial class ChallengeProfileStripView : ContentView
	{
		public static readonly BindableProperty ChallengeProperty =
			BindableProperty.Create("Challenge", typeof(Challenge), typeof(ChallengeProfileStripView), null);

		public Challenge Challenge
		{
			get
			{
				return (Challenge)GetValue(ChallengeProperty);
			}
			set
			{
				SetValue(ChallengeProperty, value);
			}
		}

		public ChallengeProfileStripView()
		{
			InitializeComponent();
		}
	}
}