using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using Microsoft.Owin.Cors;
using Budget.API.App_Start;

[assembly: OwinStartup(typeof(Budget.API.Startup))]

namespace Budget.API
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            ConfigureAuth(app);
            HttpConfiguration config = new HttpConfiguration();
            app.UseWebApi(config);
            WebApiConfig.Register(config);
            UnityConfig.RegisterComponents(config);
            JsonConfig.DateFormat(config);
        }
    }
}
