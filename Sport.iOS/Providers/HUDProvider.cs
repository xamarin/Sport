using BigTed;
using Sport.Shared;
using Xamarin.Forms;

[assembly: Dependency(typeof(Sport.iOS.HUDProvider))]

namespace Sport.iOS
{
	public class HUDProvider : IHUDProvider
	{
		public async void DisplayProgress(string message)
		{
			if(string.IsNullOrWhiteSpace(message))
			{
				BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Black);
			}
			else
			{
				BTProgressHUD.Show(message, -1, ProgressHUD.MaskType.Black);
			}
		}

		public void DisplaySuccess(string message)
		{
			BTProgressHUD.ShowSuccessWithStatus(message);
		}

		public void DisplayError(string message)
		{
			BTProgressHUD.ShowErrorWithStatus(message);
		}

		public void Dismiss()
		{
			BTProgressHUD.Dismiss();
		}
	}
}