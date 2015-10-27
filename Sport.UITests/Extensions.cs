using System;
using System.Linq;
using System.Threading;
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using System.Text;
using System.Diagnostics;

namespace Xamarin.TestCloud.Extensions
{
	public static class Extensions
	{
		public static void Back(this IApp app, Platform platform, bool screenshot = true)
		{
			if(platform == Platform.Android)
			{
				app.Back();

				if(screenshot)
					app.Screenshot("Back");
			}
			else if(platform == Platform.iOS)
			{
				app.Tap("Back");

				if(screenshot)
					app.Screenshot("Back");
			}
		}

		public static void EnterText(this IApp app, string marked, string text, string screenshot)
		{
			app.EnterText(marked, text);
			app.Screenshot(screenshot);
		}

		public static void Tap(this IApp app, string screenshot, string marked)
		{
			app.Screenshot(screenshot);
			app.Tap(marked);
		}

		public static void EnterText(this IApp app, Func<AppQuery, AppWebQuery> lambda, string text, string screenshot)
		{
			app.EnterText(lambda, text);
			app.Screenshot(screenshot);
		}

		public static void EnterText(this IApp app, Func<AppQuery, AppQuery> lambda, string text, string screenshot)
		{
			app.EnterText(lambda, text);
			app.Screenshot(screenshot);
		}

		public static void Tap(this IApp app, string screenshot, Func<AppQuery, AppQuery> lambda)
		{
			app.Screenshot(screenshot);
			app.Tap(lambda);
		}

		public static void Tap(this IApp app, Func<AppQuery, AppQuery> lambda, string screenshot)
		{
			app.Tap(lambda);
			app.Screenshot(screenshot);
		}

		public static void Tap(this IApp app, string screenshot, Func<AppQuery, AppWebQuery> lambda)
		{
			app.Screenshot(screenshot);
			app.Tap(lambda);
		}

		public static void Tap(this IApp app, Func<AppQuery, AppWebQuery> lambda, string screenshot)
		{
			app.Tap(lambda);
			app.Screenshot(screenshot);
		}

		public static string ToReplString(this AppResult[] result)
		{
			var sb = new StringBuilder();
			var index = 0;

			foreach(var res in result)
			{
				var innerSb = new StringBuilder();
				innerSb.AppendLine("{");
				innerSb.AppendLine(string.Format("    Index         - {0}", index));
				innerSb.AppendLine(string.Format("    Class         - {0}", res.Class));
				innerSb.AppendLine(string.Format("    Description   - {0}", res.Description));

				if(res.Text != null)
					innerSb.AppendLine(string.Format("    Text           - {0}", res.Text));

				innerSb.AppendLine(string.Format("    ID            - {0}", res.Id));
				innerSb.AppendLine(string.Format("    Rect          - {0} x {1}, {2} x {3}", res.Rect.X, res.Rect.Y, res.Rect.Width, res.Rect.Height));
				innerSb.AppendLine("}");
				innerSb.AppendLine("");

				sb.Append(innerSb.ToString());
				index++;
			}

			return sb.ToString();
		}

		public static string ToString(this AppWebResult[] result, bool repl)
		{
			var sb = new StringBuilder();
			var index = 0;

			foreach(var res in result)
			{
				var innerSb = new StringBuilder();
				innerSb.AppendLine("{");
				innerSb.AppendLine(string.Format("    Index         - {0}", index));
				innerSb.AppendLine(string.Format("    Class         - {0}", res.Class));
				innerSb.AppendLine(string.Format("    NodeName      - {0}", res.NodeName));
				innerSb.AppendLine(string.Format("    TextContent   - {0}", res.TextContent));

				if(res.TextContent != null)
					innerSb.AppendLine(string.Format("    TextContent   - {0}", res.TextContent));

				innerSb.AppendLine(string.Format("    ID            - {0}", res.Id));
				innerSb.AppendLine(string.Format("    Rect          - {0} x {1}, {2} x {3}", res.Rect.X, res.Rect.Y, res.Rect.Width, res.Rect.Height));
				innerSb.AppendLine(string.Format("    Html          - {0}", res.Html));
				innerSb.AppendLine("}");
				innerSb.AppendLine("");

				sb.Append(innerSb.ToString());
				index++;
			}

			return sb.ToString();
		}

	}
}