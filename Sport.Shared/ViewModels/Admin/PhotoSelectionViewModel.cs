using Xamarin.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sport.Shared
{
	public class PhotoSelectionViewModel : BaseViewModel
	{
		public League League
		{
			get;
			set;
		}

		public List<string> Photos
		{
			get;
			set;
		}

		async public Task GetPhotos(string keyword)
		{
			Photos = await FlikrService.Instance.SearchPhotos(keyword);
			SetPropertyChanged("Photos");
		}
	}
}