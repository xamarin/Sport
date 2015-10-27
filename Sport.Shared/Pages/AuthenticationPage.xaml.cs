using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sport.Shared
{
	public partial class AuthenticationPage : AuthenticationPageXaml
	{
		public AuthenticationPage()
		{
			Initialize();
		}

		protected override void Initialize()
		{
			InitializeComponent();
			Title = "Authenticating";
		}

		async public Task<bool> AttemptToAuthenticateAthlete(bool force = false)
		{
			await ViewModel.AuthenticateCompletely();

			if(App.CurrentAthlete != null)
			{
				MessagingCenter.Send<App>(App.Current, Messages.AuthenticationComplete);
			}

			return App.CurrentAthlete != null;
		}
	}

	public partial class AuthenticationPageXaml : BaseContentPage<AuthenticationViewModel>
	{
	}
}