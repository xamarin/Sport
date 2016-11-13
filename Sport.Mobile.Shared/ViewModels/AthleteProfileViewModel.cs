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

		bool _enablePushNotifications;
		public bool EnablePushNotifications
		{
			get
			{
				return Settings.EnablePushNotifications;
			}
			set
			{
				SetPropertyChanged(ref _enablePushNotifications, value);
				Settings.EnablePushNotifications = _enablePushNotifications;
			}
		}

		public Task<bool> RegisterForPushNotifications()
		{
			var tcs = new TaskCompletionSource<bool>();

			MessagingCenter.Subscribe<App>(this, Messages.RegisteredForRemoteNotifications, async (app) => {
				MessagingCenter.Unsubscribe<App>(this, Messages.RegisteredForRemoteNotifications);

				if(App.Instance.CurrentAthlete.DeviceToken != null)
				{
					App.Instance.CurrentAthlete.IsDirty = true;
					var task = AzureService.Instance.UpdateAthleteNotificationHubRegistration(App.Instance.CurrentAthlete, true, true);
					await RunSafe(task);
					NotifyPropertiesChanged();
				}

				tcs.TrySetResult(App.Instance.CurrentAthlete.DeviceToken != null);
				Device.BeginInvokeOnMainThread(() =>
				{
					IsBusy = false;
				});
			});

			IsBusy = true;
			var push = DependencyService.Get<IPushNotifications>();
			push.RegisterForPushNotifications();

			return tcs.Task;
		}
	}
}