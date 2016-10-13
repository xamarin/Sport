using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using Owin;
using Sport.Service.Models;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Web.Http;

namespace Sport.Service
{
	public partial class Startup
	{
		public static bool IsDemoMode
		{
			get;
			set;
		}

		public static int MaxLeagueMembershipCount
		{
			get;
			set;
		}

		public static void ConfigureMobileApp(IAppBuilder app)
		{
			HttpConfiguration config = new HttpConfiguration();
			config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
			config.MapHttpAttributeRoutes();

			new MobileAppConfiguration()
				.UseDefaultConfiguration()
				.ApplyTo(config);

			// Use Entity Framework Code First to create database tables based on your DbContext
			MobileAppSettingsDictionary settings = config.GetMobileAppSettingsProvider().GetMobileAppSettings();

			if(string.IsNullOrEmpty(settings.HostName))
			{
				app.UseAppServiceAuthentication(new AppServiceAuthenticationOptions
				{
					// This middleware is intended to be used locally for debugging. By default, HostName will
					// only have a value when running in an App Service application.
					SigningKey = ConfigurationManager.AppSettings["SigningKey"],
					ValidAudiences = new[] { ConfigurationManager.AppSettings["ValidAudience"] },
					ValidIssuers = new[] { ConfigurationManager.AppSettings["ValidIssuer"] },
					TokenHandler = config.GetAppServiceTokenHandler()
				});
			}

			bool isDemoMode;
			var boolString = ConfigurationManager.AppSettings["IsDemoMode"];
			if (bool.TryParse(boolString, out isDemoMode))
				IsDemoMode = isDemoMode;

			int maxCount;
			var intString = ConfigurationManager.AppSettings["MaxLeagueMembershipCount"];
			if (int.TryParse(intString, out maxCount))
				MaxLeagueMembershipCount = maxCount;

			app.UseWebApi(config);

			var migrator = new DbMigrator(new Configuration());
			migrator.Update();
		}
	}

	internal sealed class Configuration : DbMigrationsConfiguration<MobileServiceContext>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = true;
			AutomaticMigrationDataLossAllowed = true;
		}
	}
}