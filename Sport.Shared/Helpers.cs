using System;
using System.Reflection;
using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace Sport.Shared
{
	public class Helpers
	{
		public static T LoadFromFile<T>(string path)
		{
			string json = null;
			if(File.Exists(path))
			{
				using(var sr = new StreamReader(path))
				{
					json = sr.ReadToEnd();
				}

				if(json != null)
				{
					try
					{
						return JsonConvert.DeserializeObject<T>(json);
					}
					catch(Exception)
					{
					}
				}
			}

			return default(T);
		}
	}

	public class GzipWebClient : WebClient
	{
		protected override WebRequest GetWebRequest(Uri address)
		{
			var request = base.GetWebRequest(address);
			if(request is HttpWebRequest)
			{
				((HttpWebRequest)request).AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
			}

			return request;
		}
	}
}