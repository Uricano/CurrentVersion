using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Cherries.Models.Queries;
using Cherries.Services;
using Cherries.Services.Interfaces;
using Cherries.WebApi.Hubs;
using Cherries.WebApi.IoC;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace Cherries.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static IWindsorContainer _container;
        public static bool isComplete = false;
        public static List<long> lstUsers = new List<long>();
        public static Dictionary<long, string> hubUsers = new Dictionary<long, string>();
        public static IWindsorContainer Container { get { return _container; } }

        public static Process compiler;
        protected void Application_PostAuthorizeRequest()
        {
            System.Web.HttpContext.Current.SetSessionStateBehavior(System.Web.SessionState.SessionStateBehavior.Required);
        }
        protected void Application_Start()
        {
            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            ConfigureWindsor(GlobalConfiguration.Configuration);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            StartProcess();
            HttpContext current = HttpContext.Current;
            Task.Factory.StartNew(() => CheckIfUpdateNeeded(current));
        }

        private void CheckIfUpdateNeeded(HttpContext current)
        {
            HttpContext.Current = current;
            //while (true)
            //{
                try
                    {
                    IOptimizationService service = _container.Resolve<IOptimizationService>();
                    isComplete = false;
                    service.UpdateSecurities(current);
                    isComplete = true;
                    _container.Release(service);
                }
                catch (Exception ex)
                {
                 
                }
                //var interval = isComplete ? int.Parse(ConfigurationManager.AppSettings["priceRefreshInterval"]) : 3000;
                //Thread.Sleep(interval);

            //}
        }
        protected void Session_End(object sender, EventArgs e)
        {
            IUserService service = _container.Resolve<IUserService>();
            var user = ((Cherries.Models.dbo.User)Session["user"]);
            if (user != null)
            {
                var userId = user.UserID;
                LogoffCommand cmd = new LogoffCommand { IP = Session["ip"].ToString(), Users = new List<long>() { userId } };
                service.Logoff(cmd);
                lstUsers.Remove(userId);
                var hub = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<TakinHub>();
                if(hubUsers.ContainsKey(userId))
                    hub.Clients.Client(hubUsers[userId]).sessionEnded();
            }



        }//Session_End

        protected void Application_End()
        {
            OptimizationService.EndProcess();
            IUserService service = _container.Resolve<IUserService>();
            LogoffCommand cmd = new LogoffCommand { Users = lstUsers };
            service.Logoff(cmd);
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            string defaultPage = "/index.html";
            string url = "https://" + Request.ServerVariables["HTTP_HOST"];
            string localPath = Request.Url.LocalPath;
            if (HttpContext.Current.Request.IsSecureConnection.Equals(false) && HttpContext.Current.Request.IsLocal.Equals(false))
            {
                if (!System.IO.File.Exists(Context.Server.MapPath(localPath)) && HttpContext.Current.Request.RawUrl.IndexOf("/api/") == -1
                    && HttpContext.Current.Request.RawUrl.IndexOf(".") == -1 && HttpContext.Current.Request.RawUrl.IndexOf("/signalr/") == -1)
                    url += defaultPage;
                else
                    url += HttpContext.Current.Request.RawUrl;

                Context.Response.Redirect(url);

            }
            else
            {
                if (!System.IO.File.Exists(Context.Server.MapPath(localPath)) && HttpContext.Current.Request.RawUrl.IndexOf("/api/") == -1
                    && HttpContext.Current.Request.RawUrl.IndexOf(".") == -1 && HttpContext.Current.Request.RawUrl.IndexOf("/signalr/") == -1)
                    Context.RewritePath(defaultPage);
            }
        }


        private static void StartProcess()
        {
            OptimizationService.StartProcess(HttpContext.Current.Server.MapPath("~/bin/TFI.CalculationEngine.exe"));            
        }

        public static void ConfigureWindsor(HttpConfiguration configuration)
        {
            _container = new WindsorContainer();

            _container.Install(FromAssembly.This());
            _container.Install(FromAssembly.Named("Cherries.Services.Bootstrapper"));
            _container.Install(FromAssembly.Named("TFI.BusinessLogic.Bootstraper"));

            _container.Kernel.Resolver.AddSubResolver(new CollectionResolver(_container.Kernel, true));

            var dependencyResolver = new WindsorDependencyResolver(_container);
            configuration.DependencyResolver = dependencyResolver;
            GlobalHost.DependencyResolver = new SignalrWindsorDependencyResolver(_container);
        }
    }
}
