using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
[assembly: OwinStartup(typeof(Cherries.WebApi.App_Start.Startup))]
namespace Cherries.WebApi.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalHost.Configuration.DefaultMessageBufferSize = 2000000;
            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = 20000000;
            GlobalHost.Configuration.TransportConnectTimeout = TimeSpan.FromSeconds(10);
            app.MapSignalR(new HubConfiguration
            {
                EnableJSONP = true,
                EnableJavaScriptProxies = true,
                EnableDetailedErrors = true
            });
        }
    }
}