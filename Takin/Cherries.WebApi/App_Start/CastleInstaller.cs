using System.Web.Http;
using Register = Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Microsoft.AspNet.SignalR.Hubs;

namespace Cherries.WebApi.App_Start
{
    public class CastleInstaller : Register.IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
              Register.Classes
              .FromThisAssembly()
              .BasedOn<ApiController>()
              .LifestylePerWebRequest()
              );

            container.Register(Register.Classes.FromThisAssembly().BasedOn<IHub>().LifestyleTransient());
        }
    }
}