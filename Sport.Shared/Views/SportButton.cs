using System;
using Xamarin.Forms;
using System.Diagnostics;

namespace Sport.Shared
{
	public class SportButton : Button
	{
		public SportButton() : base()
		{
			const int _animationTime = 100;
			Clicked += async(sender, e) =>
			{
				var btn = (SportButton)sender;
				await btn.ScaleTo(1.2, _animationTime);
				btn.ScaleTo(1, _animationTime);
			};
		}
	}
}