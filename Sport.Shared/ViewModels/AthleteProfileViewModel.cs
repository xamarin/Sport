using System;
using System.Windows.Input;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Sport.Shared
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
				var task = AzureService.Instance.SaveAthlete(Athlete);
				await RunSafe(task);
				return !task.IsFaulted;
			}
		}

		async public Task<bool> DeleteAthlete()
		{
			var task = AzureService.Instance.DeleteAthlete(Athlete.Id);
			await RunSafe(task);
			return !task.IsFaulted;
		}

		public void RegisterForPushNotifications(Action onComplete)
		{
			MessagingCenter.Subscribe<App>(this, Messages.RegisteredForRemoteNotifications, async(app) =>
			{
				if(App.CurrentAthlete.DeviceToken != null)
				{
					App.CurrentAthlete.IsDirty = true;
					var task = AzureService.Instance.UpdateAthleteNotificationHubRegistration(App.CurrentAthlete, true, true);
					await RunSafe(task);
				}

				onComplete();

				Device.BeginInvokeOnMainThread(() =>
				{
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