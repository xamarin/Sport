using System;
using Xamarin.Forms;

namespace Sport.Mobile.Shared
{
	public interface IStatusBarManager
	{
		void SetColor(string color);
		void SetColor(Color color);
	}
}
