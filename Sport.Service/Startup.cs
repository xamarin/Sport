using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using System;

[assembly: OwinStartup(typeof(Sport.Service.Startup))]

namespace Sport.Service
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}