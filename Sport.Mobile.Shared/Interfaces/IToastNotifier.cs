using System;
using System.Threading.Tasks;

namespace Sport.Mobile.Shared
{
	public interface IToastNotifier
	{
		Task<bool> Notify(ToastNotificationType type, string title, string description, TimeSpan duration, object context = null);

		void HideAll();
	}

	public enum ToastNotificationType
	{
		Info,
		Success,
		Error,
		Warning,
	}
}