using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sport.Mobile.Shared
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
			await ViewModel.Authenticate();

			if(App.Instance.CurrentAthlete != null)
			{
				MessagingCenter.Send<App>(App.Instance, Messages.AuthenticationComplete);
			}

			return App.Instance.CurrentAthlete != null;
		}
	}

	public partial class AuthenticationPageXaml : BaseContentPage<AuthenticationViewModel>
	{
	}
}