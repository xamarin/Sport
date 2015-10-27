using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Sport.Shared
{
	public class FlikrService
	{
		static FlikrService _instance;

		public static FlikrService Instance
		{
			get
			{
				return _instance ?? (_instance = new FlikrService());
			}
		}

		async public Task<List<string>> SearchPhotos(string keyword)
		{
			string url = "https://api.flickr.com/services/rest/?method=flickr.photos.search&tag_mode=all&api_key={1}&tags={0},B%26W&format=json&nojsoncallback=1";
			url = url.Fmt(keyword, Keys.FlikrApiKey);

			using(var client = new HttpClient())
			{
				var json = await client.GetStringAsync(url);

				var result = JsonConvert.DeserializeObject<FlikrResult>(json);
				var list = new List<string>();

				foreach(var photo in result.Photos.Photo)
				{
					var photoUrl = "https://farm{0}.staticflickr.com/{1}/{2}_{3}_b.jpg".Fmt(photo.Farm, photo.Server, photo.Id, photo.Secret);
					list.Add(photoUrl);
				}

				return list;
			}
		}
	}
}

