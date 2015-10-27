using System;
using Xamarin.Forms;
using System.Linq;
using Version.Plugin;

namespace Sport.Shared
{
	public partial class AboutPage : AboutPageXaml
	{
		public AboutPage()
		{
			Initialize();
			Title = "About";
		}

		protected override void Initialize()
		{
			InitializeComponent();
			versionLabel.Text = "version {0}".Fmt(CrossVersion.Current.Version);
		}

		void HandleXamarinClicked(object sender, EventArgs e)
		{
			Device.OpenUri(new Uri("http://xamarin.com/forms"));
		}

		void HandleViewSourceClicked(object sender, EventArgs e)
		{
			Device.OpenUri(new Uri(Keys.SourceCodeUrl));
		}
	}

	public partial class AboutPageXaml : BaseContentPage<BaseViewModel>
	{

	}
}