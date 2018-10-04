using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ness.DataAccess.Fluent
{
    public class SessionHandler
    {
        /// <summary>
        /// Run Trasactions queries
        /// </summary>
        /// <param name="action">delegate function of the query</param>
        public void Update(Action<ISession> action)
        {
            NHibernateHelper helper = new Ness.DataAccess.Fluent.NHibernateHelper();
            using (var session = helper.GetSession())
            {
                using (var tx = session.BeginTransaction())//seems needless
                {
                    try
                    {
                        action(session);

                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                    }
                }
            }
        }

        /// Run Select queries
        /// </summary>
        /// <param name="query">delegate function of the query</param>
        public void Query(Action<ISession> query)
        {
            NHibernateHelper helper = new Ness.DataAccess.Fluent.NHibernateHelper();
            using (var session = helper.GetSession())
            {
                try
                {
                    query(session);
                }
                catch (Exception ex)
                {
                    string s = ex.Message;
                }
            }
        }

        public void ExecuteParallel<T>(List<T> lst, Action<ISession, T> execute)
        {
            NHibernateHelper helper = new Ness.DataAccess.Fluent.NHibernateHelper();
            Parallel.ForEach(lst, item =>
            {
                using (var session = helper.GetSession())
                {
                    execute(session, item);
                }
            });
        }
    }
}
