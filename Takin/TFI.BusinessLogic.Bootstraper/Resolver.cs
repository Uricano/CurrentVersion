using Castle.MicroKernel;
using Castle.Windsor;
using Castle.Windsor.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.BusinessLogic.Bootstraper
{
    public class Resolver
    {
        private static readonly WindsorContainer container = new WindsorContainer();
        static Resolver()
        {
            container.Install(FromAssembly.This());
        }

        public static T Resolve<T>()
        {
            return container.Resolve<T>();
        }

        public static void Release(object component)
        {
            container.Release(component);
        }
    }
}
