using System;
using UIKit;
using Sport.Shared;

namespace Sport.iOS
{
	public class Application
	{
		//This is the main entry point of the application.
		static void Main(string[] args)
		{
			try
			{
				UIApplication.Main(args, null, "AppDelegate");
			}
			catch(Exception e)
			{
				var ex = e.GetBaseException();
				Console.WriteLine("**SPORT MAIN EXCEPTION**\n\n" + ex);
				InsightsManager.Report(ex, Xamarin.Insights.Severity.Critical);
				throw;
			}

			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				try
				{
					var ex = ((Exception)e.ExceptionObject).GetBaseException();
					Console.WriteLine("**SPORT UNHANDLED EXCEPTION**\n\n" + ex);
					InsightsManager.Report(ex, Xamarin.Insights.Severity.Critical);
				}
				catch
				{
				}
			};
		}
	}
}