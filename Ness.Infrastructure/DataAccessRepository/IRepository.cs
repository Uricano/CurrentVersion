using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ness.DataAccess.Repository
{
    public interface IRepository : IDisposable
    {
        void Execute(Action<ISession> execute);
        void ExecuteSp(string spName, Dictionary<string, Tuple<object, NHibernate.Type.IType>> parameters);
        IList<T> ExecuteSp<T>(string spName, Dictionary<string, Tuple<object, NHibernate.Type.IType>> parameters);
        void ExecuteTransaction(Action<ISession> execute);
        void ExecuteTransactionStateLess(Action<IStatelessSession> execute, int size);

    }
}
