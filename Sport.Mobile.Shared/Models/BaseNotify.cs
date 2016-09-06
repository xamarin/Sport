using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace Sport.Mobile.Shared
{
	public class BaseNotify : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		internal bool SetPropertyChanged<T>(ref T currentValue, T newValue, [CallerMemberName] string propertyName = "")
		{
			return PropertyChanged.SetProperty(this, ref currentValue, newValue, propertyName);
		}

		internal void SetPropertyChanged(string propertyName)
		{
			if(PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}

namespace System.ComponentModel
{
	public static class BaseNotify
	{
		//Just adding some new funk.tionality to System.ComponentModel
		public static bool SetProperty<T>(this PropertyChangedEventHandler handler, object sender, ref T currentValue, T newValue, [CallerMemberName] string propertyName = "")
		{
			if(EqualityComparer<T>.Default.Equals(currentValue, newValue))
				return false;
			
			currentValue = newValue;

			var dirty = sender as Sport.Mobile.Shared.IDirty;

			if(dirty != null)
				dirty.IsDirty = true;

			if(handler == null)
				return true;

			handler.Invoke(sender, new PropertyChangedEventArgs(propertyName));
			return true;
		}
	}
}