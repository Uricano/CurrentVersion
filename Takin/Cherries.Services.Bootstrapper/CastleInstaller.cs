using Castle.MicroKernel.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Cherries.Services.Interfaces;

namespace Cherries.Services.Bootstrapper
{
    public class CastleInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
               Classes.FromAssemblyNamed("Cherries.Services")
               .BasedOn<IServiceBase>()
               .WithService.AllInterfaces()
               .LifestyleTransient());
            //container.Register(
            //    Component.For<IUserService>().ImplementedBy<UserService>().LifestyleTransient(),
            //    Component.For<IBackTestingService>().ImplementedBy<BackTestingService>().LifestyleTransient(),
            //    Component.For<IOptimizationService>().ImplementedBy<OptimizationService>().LifestyleTransient(),
            //    Component.For<IPortfolioService>().ImplementedBy<PortfolioService>().LifestyleTransient(),
            //     Component.For<ISecuritiesService>().ImplementedBy<SecuritiesService>().LifestyleTransient()
            //    );
        }
    }
}
