using Xamarin.UITest;
using Xamarin.UITest.Configuration;
using Xamarin.UITest.Utils;
using System;

namespace Sport.UITests
{
	public class AppInitializer
	{
		public static IApp StartApp(Platform platform)
		{
			if(platform == Platform.Android)
			{
				return ConfigureApp.Android.WaitTimes(new DefaultWaitTimes()).StartApp();
				//	return ConfigureApp.Android.ApkFile("~/Desktop/com.xamarin.sport.apk").StartApp();
			}

			return ConfigureApp.iOS.StartApp(AppDataMode.Clear);
		}
	}

	class DefaultWaitTimes : IWaitTimes
	{
		public TimeSpan GestureWaitTimeout
		{
			get
			{
				if(TestEnvironment.IsTestCloud)
					return TimeSpan.FromMinutes(5);

				return TimeSpan.FromSeconds(30);
			}
		}

		public TimeSpan WaitForTimeout
		{
			get
			{
				if(TestEnvironment.IsTestCloud)
					return TimeSpan.FromMinutes(5);

				return TimeSpan.FromSeconds(30);
			}
		}
	}
}