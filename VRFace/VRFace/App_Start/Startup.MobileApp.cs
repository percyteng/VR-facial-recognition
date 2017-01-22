namespace VRFace
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Entity;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Web.Http;
    using Microsoft.Azure.Mobile.Server;
    using Microsoft.Azure.Mobile.Server.Authentication;
    using Microsoft.Azure.Mobile.Server.Config;
    using Microsoft.Azure.Mobile.Server.Tables.Config;
    using Owin;
    using VRFace.DataObjects;
    using VRFace.IO;
    using VRFace.Models;
    using System.Diagnostics;

    public partial class Startup
    {
        public static void ConfigureMobileApp(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            new MobileAppConfiguration()
                .MapApiControllers()
                .AddTables(                               // from the Tables package
                    new MobileAppTableConfiguration()
                        .MapTableControllers()
                        .AddEntityFramework())             // from the Entity package
                .AddPushNotifications()                   // from the Notifications package
                .MapLegacyCrossDomainController()         // from the CrossDomain package
                .ApplyTo(config);

            // Add JSON
            config.Formatters.JsonFormatter.SupportedMediaTypes
                .Add(new MediaTypeHeaderValue("text/html"));
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.JsonFormatter.MediaTypeMappings.Add(new RequestHeaderMapping("Accept",
                              "text/html",
                              StringComparison.InvariantCultureIgnoreCase,
                              true,
                              "application/json"));

            // Use Entity Framework Code First to create database tables based on your DbContext
            Database.SetInitializer(new MobileServiceInitializer());

            MobileAppSettingsDictionary settings = config.GetMobileAppSettingsProvider().GetMobileAppSettings();

            if (string.IsNullOrEmpty(settings.HostName))
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

            app.UseWebApi(config);

            //Trace.TraceError(ConfigurationManager.AppSettings["GroupName"]);
        }
    }
}
