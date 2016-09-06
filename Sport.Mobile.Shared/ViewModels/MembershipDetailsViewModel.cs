using System;
using System.Windows.Input;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Sport.Mobile.Shared
{
	public class MembershipDetailsViewModel : MembershipViewModel
	{
		//async public Task DeleteMembership()
		//{
		//	await RunSafe(AzureService.Instance.DeleteMembership(Membership));
		//}

		async public Task RefreshMembership()
		{
			using(new Busy(this))
			{
				await AzureService.Instance.MembershipManager.GetItemAsync(Membership.Id, true);
				//var task = AzureService.Instance.GetMembershipById(MembershipId, true);
				//await RunSafe(task);

				//if(task.IsFaulted)
				//	return;
			}

			NotifyPropertiesChanged();
		}
	}
}