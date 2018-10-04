using NHibernate;
using NHibernate.Linq;
using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Ness.DataAccess.Fluent;

namespace Ness.DataAccess.Repository
{
    public class Repository : IDisposable, IRepository
    {
        private ISessionFactoryHelper helper;// = new NHibernateHelper();
        
        private ISession session;
        private IStatelessSession stateLessSession;
        
        public Repository(ISessionFactoryHelper facoryHelper)
        {
            helper = facoryHelper;
        }

        public void Execute(Action<ISession> execute)
        {
            using (session = helper.GetSession())
            {
                execute(session);
            }
        }
        
        public void ExecuteSp(string spName, Dictionary<string, Tuple<object, NHibernate.Type.IType>> parameters)
        {
            using (session = helper.GetSession())
            {
                var query = CreateQuery(spName, parameters);
                query.ExecuteUpdate();
            }
        }

        public IList<T> ExecuteSp<T>(string spName, Dictionary<string,Tuple<object,NHibernate.Type.IType>> parameters)
        {
             using (session = helper.GetSession())
            {
                var query = CreateQuery(spName, parameters);
                query.SetTimeout(3600);
                query.SetResultTransformer(Transformers.AliasToBean<T>());
                return query.List<T>();
            }
            
        }
        public void ExecuteTransaction(Action<ISession> execute)
        {
            using (session = helper.GetSession())
            {
                //session.CommandText = "SET NOCOUNT OFF";
                using (var tx = session.BeginTransaction())
                {
                    try
                    {
                        execute(session);
                        tx.Commit();
                    }
                    catch (Exception e)
                    {
                        tx.Rollback();
                        throw e;
                    }
                }
                
            }
        }

        public void ExecuteTransactionStateLess(Action<IStatelessSession> execute, int size)
        {
            using (stateLessSession = helper.GetStateLessSession())
            {
                using (var tx = stateLessSession.BeginTransaction())
                {
                    try
                    {
                        stateLessSession.SetBatchSize(size);
                        execute(stateLessSession);
                        tx.Commit();
                    }
                    catch (Exception e)
                    {
                        tx.Rollback();
                        throw e;
                    }
                }

            }
        }

        private IQuery CreateQuery(string spName, Dictionary<string, Tuple<object, NHibernate.Type.IType>> parameters)
        {
            string queryStr = "exec " + spName;

            if (parameters.Count > 0)
            {
                queryStr += " :" + string.Join(",:", parameters.Keys.ToArray());
            }
            var query = session.CreateSQLQuery(queryStr);
                        
            
            foreach (var item in parameters)
            {
                query.SetParameter(item.Key, item.Value.Item1, item.Value.Item2);
                
            }
            return query;
        }
        public void Dispose()
        {
            if (session != null) session.Dispose();
            GC.Collect();
        }
    }
}
