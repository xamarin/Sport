
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Sport.Mobile.Shared
{
	public class AthleteManager : BaseManager<Athlete>
	{
		public override string Identifier => "Athlete";

		public Task<Athlete> GetAthleteByEmail(string email)
		{
			return new Task<Athlete>(() => {
				//We hit the backend table instead of local store since we might not have data
				var list = AzureService.Instance.Client.GetTable<Athlete>().Where(a => a.Email == email).ToListAsync().Result;
				var athlete = list.FirstOrDefault();
				return athlete;
			});
		}

		async public override Task<bool> UpdateAsync(Athlete item)
		{			
			var result = await base.UpdateAsync(item);

			if(item.Id == App.Instance.CurrentAthlete?.Id)
			{
				Task.Run(async () => {
					App.Instance.CurrentAthlete = await AzureService.Instance.AthleteManager.Table.LookupAsync(item.Id);
				}).Wait();
			}

			return result;
		}
	}
}