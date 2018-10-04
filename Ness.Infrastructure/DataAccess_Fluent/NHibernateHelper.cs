using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ness.DataAccess.Fluent
{
    public class NHibernateHelper : ISessionFactoryHelper
    {
        
        private static ISessionFactory _sessionFactory;
        [ThreadStatic]
        private static ISession _currentSession;

        public NHibernateHelper()
        {
            getSessionFactory();
        }
        private ISessionFactory getSessionFactory()
        {
            lock (this)
            {
                if (_sessionFactory == null)
                {
                    try
                    {
                        FluentConfiguration configuration = GetConfiguration();
                        _sessionFactory = configuration.BuildSessionFactory();
                    }
                    catch (Exception e)
                    {
                        var s = e.Message;
                    }
                }

                return _sessionFactory;
            }
        }

        private FluentConfiguration GetConfiguration()
        {
            var config = Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2012.DefaultSchema("dbo")
                  .ConnectionString(c => c.FromConnectionStringWithKey("ConnStr")
                ).ShowSql())
                .Mappings(m =>
                          m.FluentMappings.AddFromAssembly(AppDomain.CurrentDomain.GetAssemblies().Single(x=> x.GetName().Name == ConfigurationManager.AppSettings["MapAsssembly"])))

                .ExposeConfiguration(cfg => new SchemaExport(cfg)
                                                .Create(true, false));
            return config;     
        }

        private ISession getExistingOrNewSession(ISessionFactory factory)
        {
            {
                if (HttpContext.Current != null)
                {
                    ISession session = HttpContext.Current.Items[GetType().FullName] as ISession;
                    if (session == null)
                    {
                        session = openSessionAndAddToContext(factory);
                    }
                    else if (!session.IsOpen)
                    {
                        session.Dispose();
                        session = openSessionAndAddToContext(factory);
                    }

                    return session;
                }

                if (_currentSession == null)
                {
                    _currentSession = factory.OpenSession();
                }
                else if (!_currentSession.IsOpen)
                {
                    _currentSession = factory.OpenSession();
                }
            }

            return _currentSession;
        }

        public ISession GetSession()
        {
            return getExistingOrNewSession(_sessionFactory);
        }

        public IStatelessSession GetStateLessSession()
        {
            return _sessionFactory.OpenStatelessSession();
        }

        private ISession openSessionAndAddToContext(ISessionFactory factory)
        {
            ISession session = factory.OpenSession();
            HttpContext.Current.Items.Remove(GetType().FullName);
            HttpContext.Current.Items.Add(GetType().FullName, session);
            return session;
        }
    }
}
