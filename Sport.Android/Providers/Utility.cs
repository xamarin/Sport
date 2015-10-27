using System;
using System.Collections.Generic;
using Android.Content;
using Android.Runtime;
using Java.Security;
using Javax.Crypto;
using Sport.Shared;
using Java.IO;

namespace Sport.Shared
{
	public static class Utility
	{
		static Dictionary<string,KeyStore> keyStores = new Dictionary<string, KeyStore>();

		static internal void SetSecured(string key, string value, string clientId, string service, string sharedGrou = null)
		{
			var ks = LoadKeyStore(clientId, service);

			var entry = new KeyStore.SecretKeyEntry(new SecretAccount(value));
			var protect = new KeyStore.PasswordProtection(service.ToCharArray());
			ks.SetEntry("{0} - {1}".Fmt(clientId, key), entry, protect);
			Save(clientId, service, ks);
		}

		static internal string GetSecured(string key, string clientId, string service, string sharedGroup)
		{
			var keyId = $"{clientId} - {key}";
			var prot = new KeyStore.PasswordProtection(service.ToCharArray());
			var ks = LoadKeyStore(clientId, service);
			var aliases = ks.Aliases();
			while(aliases.HasMoreElements)
			{
				var alias = aliases.NextElement().ToString();
				if(alias == keyId)
				{
					var e = ks.GetEntry(alias, prot) as KeyStore.SecretKeyEntry;
					if(e != null)
					{
						var bytes = e.SecretKey.GetEncoded();
						var serialized = System.Text.Encoding.UTF8.GetString(bytes);
						return serialized;
					}
				}
			}

			return "";
		}

		static readonly object fileLock = new object();

		static KeyStore LoadKeyStore(string clientId, string key)
		{
			var context = global::Android.App.Application.Context;
			KeyStore ks;
			if(keyStores.TryGetValue(clientId, out ks))
				return ks;
			var pw = key.ToCharArray();
			ks = KeyStore.GetInstance(KeyStore.DefaultType);

			var prot = new KeyStore.PasswordProtection(pw);

			try
			{
				lock(fileLock)
				{
					using(var s = context.OpenFileInput(clientId))
					{
						ks.Load(s, pw);
					}
				}
			}
			catch(FileNotFoundException)
			{
				//ks.Load (null, Password);
				LoadEmptyKeyStore(ks, pw);
			}
			keyStores[clientId] = ks;
			return ks;
		}

		static void Save(string clientid, string service, KeyStore ks)
		{
			var context = global::Android.App.Application.Context;
			lock(fileLock)
			{
				using(var s = context.OpenFileOutput(clientid, FileCreationMode.Private))
				{
					ks.Store(s, service.ToCharArray());
				}
			}
		}

		class SecretAccount : Java.Lang.Object, ISecretKey
		{
			byte[] bytes;

			public SecretAccount(string data)
			{
				bytes = System.Text.Encoding.UTF8.GetBytes(data);
			}

			public byte[] GetEncoded()
			{
				return bytes;
			}

			public string Algorithm
			{
				get
				{
					return "RAW";
				}
			}

			public string Format
			{
				get
				{
					return "RAW";
				}
			}
		}

		static IntPtr id_load_Ljava_io_InputStream_arrayC;

		/// <summary>
		/// Work around Bug https://bugzilla.xamarin.com/show_bug.cgi?id=6766
		/// </summary>
		static void LoadEmptyKeyStore(KeyStore ks, char[] password)
		{
			if(id_load_Ljava_io_InputStream_arrayC == IntPtr.Zero)
			{
				id_load_Ljava_io_InputStream_arrayC = JNIEnv.GetMethodID(ks.Class.Handle, "load", "(Ljava/io/InputStream;[C)V");
			}
			IntPtr intPtr = IntPtr.Zero;
			IntPtr intPtr2 = JNIEnv.NewArray(password);
			JNIEnv.CallVoidMethod(ks.Handle, id_load_Ljava_io_InputStream_arrayC, new JValue[] {
				new JValue(intPtr),
				new JValue(intPtr2)
			});
			JNIEnv.DeleteLocalRef(intPtr);
			if(password != null)
			{
				JNIEnv.CopyArray(intPtr2, password);
				JNIEnv.DeleteLocalRef(intPtr2);
			}
		}
	}
}

