using System;
using System.Windows.Input;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Sport.Mobile.Shared
{
	public class AthleteProfileViewModel : AthleteViewModel
	{
		public ICommand SaveCommand
		{
			get
			{
				return new Command(async(param) =>
					await SaveAthlete());
			}
		}

		async public Task<bool> SaveAthlete()
		{
			using(new Busy(this))
			{
				var success = await AzureService.Instance.AthleteManager.UpsertAsync(Athlete);
				NotifyPropertiesChanged();
				return success;
			}
		}

		public void RegisterForPushNotifications(Action onComplete)
		{
			//var e = new Exception("Push notifications are disabled in demo mode.");
			//MessagingCenter.Send(new object(), Messages.ExceptionOccurred, e);

			MessagingCenter.Subscribe<App>(this, Messages.RegisteredForRemoteNotifications, async (app) => {
				if(App.Instance.CurrentAthlete.DeviceToken != null)
				{
					App.Instance.CurrentAthlete.IsDirty = true;
					var task = AzureService.Instance.UpdateAthleteNotificationHubRegistration(App.Instance.CurrentAthlete, true, true);
					await RunSafe(task);
					NotifyPropertiesChanged();
				}

				onComplete();

				Device.BeginInvokeOnMainThread(() => {
					IsBusy = false;
					MessagingCenter.Unsubscribe<App>(this, Messages.RegisteredForRemoteNotifications);
				});
			});

			IsBusy = true;
			var push = DependencyService.Get<IPushNotifications>();
			push.RegisterForPushNotifications();
		}
	}
}