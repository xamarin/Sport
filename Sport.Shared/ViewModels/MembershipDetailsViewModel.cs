using System;
using System.Windows.Input;
using Xamarin.Forms;
using System.Threading.Tasks;

[assembly: Dependency(typeof(Sport.Shared.MembershipDetailsViewModel))]

namespace Sport.Shared
{
	public class MembershipDetailsViewModel : MembershipViewModel
	{
		async public Task DeleteMembership()
		{
			await RunSafe(AzureService.Instance.DeleteMembership(Membership.Id));
		}

		async public Task RefreshMembership()
		{
			using(new Busy(this))
			{
				var task = AzureService.Instance.GetMembershipById(MembershipId, true);
				await RunSafe(task);

				if(task.IsFaulted)
					return;
			}
			NotifyPropertiesChanged();
		}
	}
}