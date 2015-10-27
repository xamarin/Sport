using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sport.Shared
{
	public class FlikrResult
	{
		[JsonProperty("photos")]
		public Photos Photos
		{
			get;
			set;
		}

		[JsonProperty("stat")]
		public string Stat
		{
			get;
			set;
		}
	}

	public class Photo
	{
		[JsonProperty("id")]
		public string Id
		{
			get;
			set;
		}

		[JsonProperty("owner")]
		public string Owner
		{
			get;
			set;
		}

		[JsonProperty("secret")]
		public object Secret
		{
			get;
			set;
		}

		[JsonProperty("server")]
		public string Server
		{
			get;
			set;
		}

		[JsonProperty("farm")]
		public int Farm
		{
			get;
			set;
		}

		[JsonProperty("title")]
		public string Title
		{
			get;
			set;
		}

		[JsonProperty("ispublic")]
		public int Ispublic
		{
			get;
			set;
		}

		[JsonProperty("isfriend")]
		public int Isfriend
		{
			get;
			set;
		}

		[JsonProperty("isfamily")]
		public int Isfamily
		{
			get;
			set;
		}
	}

	public class Photos
	{

		[JsonProperty("page")]
		public int Page
		{
			get;
			set;
		}

		[JsonProperty("pages")]
		public string Pages
		{
			get;
			set;
		}

		[JsonProperty("perpage")]
		public int Perpage
		{
			get;
			set;
		}

		[JsonProperty("total")]
		public string Total
		{
			get;
			set;
		}

		[JsonProperty("photo")]
		public IList<Photo> Photo
		{
			get;
			set;
		}
	}
}

