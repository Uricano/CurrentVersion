using Castle.MicroKernel.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CastleClasses = Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Ness.DataAccess.Repository;
using Ness.DataAccess.Fluent;
using TFI.BusinessLogic.Interfaces;

namespace TFI.BusinessLogic.Bootstraper
{
    public class CastleInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
               Component.For<ISessionFactoryHelper>().ImplementedBy<NHibernateHelper>().LifestyleTransient()
               );

            container.Register(
               Component.For<IRepository>().ImplementedBy<Repository>().LifestyleTransient()
               );

            container.Register(
               CastleClasses.Classes
               .FromAssemblyNamed("TFI.BusinessLogic")
               .BasedOn(typeof(IBaseBL))
               .WithService.AllInterfaces()
               .LifestyleTransient()
               );

        }
    }
}
