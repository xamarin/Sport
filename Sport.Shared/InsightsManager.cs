using System;
using Xamarin;
using System.Collections.Generic;

namespace Sport.Shared
{
	public static class InsightsManager
	{
		public static bool IsEnabled
		{
			get
			{
				return !string.IsNullOrWhiteSpace(Keys.InsightsApiKey);
			}
		}

		public static void Identify(string id, Dictionary<string, string> meta)
		{
			if(!IsEnabled)
				return;

			Insights.Identify(id, meta);
		}

		public static void Report(Exception e, Insights.Severity severity = Insights.Severity.Warning)
		{
			if(!IsEnabled)
				return;

			Insights.Report(e, severity);
		}

		public static void Track(string pageId, Dictionary<string, string> meta)
		{
			if(!IsEnabled)
				return;

			Insights.Track(pageId, meta);
		}
	}
}