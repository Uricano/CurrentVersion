using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ness.DataAccess.Fluent
{
    public interface ISessionFactoryHelper
    {
        ISession GetSession();
        IStatelessSession GetStateLessSession();
    }
}
