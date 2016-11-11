using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace Sport.Mobile.Shared
{
	public class BaseModel : BaseNotify, IDirty
	{
		string _id;

		[JsonProperty("id")]
		public string Id
		{
			get
			{
				return _id;
			}
			set
			{
				SetPropertyChanged(ref _id, value);
			}
		}

		DateTime? _updatedAt;

		public DateTime? UpdatedAt
		{
			get
			{
				return _updatedAt;
			}
			set
			{
				SetPropertyChanged(ref _updatedAt, value);
			}
		}

		DateTime? _createdAt;

		[CreatedAt]
		public DateTime? CreatedAt
		{
			get
			{
				return _createdAt;
			}
			set
			{
				SetPropertyChanged(ref _createdAt, value);
			}
		}

		string _version;

		[Version]
		public string Version
		{
			get
			{
				return _version;
			}
			set
			{
				SetPropertyChanged(ref _version, value);	
			}
		}

		[JsonIgnore]
		public bool IsDirty
		{
			get;
			set;
		}

		public virtual void LocalRefresh()
		{
		}

		public virtual void NotifyPropertiesChanged([CallerMemberName] string caller = "")
		{
			Debug.WriteLine($"NotifyPropertiesChanged called for {GetType().Name} by {caller}");
		}
	}
}