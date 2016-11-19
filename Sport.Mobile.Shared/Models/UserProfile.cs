using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sport.Mobile.Shared
{
	public class GoogleUserProfile : IUserProfile
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
		public string PhotoUrl
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


	public class ActiveDirectoryUserProfile : IUserProfile
	{
		public string Email
		{
			get;
			set;
		}

		public string Id
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public string PhotoUrl
		{
			get;
			set;
		}
	}

	public class UserClaim
	{
		[JsonProperty("typ")]
		public string Typ
		{
			get; set;
		}

		[JsonProperty("val")]
		public string Val
		{
			get; set;
		}
	}

	public class AppServiceIdentity
	{
		[JsonProperty("id_token")]
		public string IdToken
		{
			get; set;
		}

		[JsonProperty("provider_name")]
		public string ProviderName
		{
			get; set;
		}

		[JsonProperty("user_claims")]
		public List<UserClaim> UserClaims
		{
			get; set;
		}

		[JsonProperty("user_id")]
		public string UserId
		{
			get; set;
		}

		[JsonProperty("access_token")]
		public string AccessToken
		{
			get; set;
		}

		[JsonProperty("refresh_token")]
		public string RefreshToken
		{
			get; set;
		}
	}
}