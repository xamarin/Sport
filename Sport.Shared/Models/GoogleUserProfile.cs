using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sport.Shared
{
	public class GoogleUserProfile
	{
		[JsonProperty("id")]
		public string Id
		{
			get;
			set;
		}

		[JsonProperty("email")]
		public string Email
		{
			get;
			set;
		}

		[JsonProperty("verified_email")]
		public bool VerifiedEmail
		{
			get;
			set;
		}

		[JsonProperty("name")]
		public string Name
		{
			get;
			set;
		}

		[JsonProperty("given_name")]
		public string GivenName
		{
			get;
			set;
		}

		[JsonProperty("family_name")]
		public string FamilyName
		{
			get;
			set;
		}

		[JsonProperty("link")]
		public string Link
		{
			get;
			set;
		}

		[JsonProperty("picture")]
		public string Picture
		{
			get;
			set;
		}

		[JsonProperty("gender")]
		public string Gender
		{
			get;
			set;
		}

		[JsonProperty("locale")]
		public string Locale
		{
			get;
			set;
		}

		[JsonProperty("hd")]
		public string Hd
		{
			get;
			set;
		}
	}

	public class Identity
	{
		[JsonProperty("access_token")]
		public string AccessToken
		{
			get;
			set;
		}

		[JsonProperty("provider")]
		public string Provider
		{
			get;
			set;
		}

		[JsonProperty("expires_in")]
		public int ExpiresIn
		{
			get;
			set;
		}

		[JsonProperty("user_id")]
		public string UserId
		{
			get;
			set;
		}

		[JsonProperty("connection")]
		public string Connection
		{
			get;
			set;
		}

		[JsonProperty("isSocial")]
		public bool IsSocial
		{
			get;
			set;
		}
	}
}