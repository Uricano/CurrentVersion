using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Cherries.TFI.BusinessLogic.DataManagement
{
    public class cDBWorker
    {
        private DbProviderFactory factory;
        private DbConnection _DbConnection;
        private DbCommand _SelectCommand;
        private DbDataAdapter _DbDataAdapter;
        public cDBWorker() { }
        public cDBWorker(DbConnection pDbConnection) 
        {
            factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            _DbConnection = pDbConnection;
            _SelectCommand = factory.CreateCommand();
            _SelectCommand.Connection = _DbConnection;
            _DbDataAdapter = factory.CreateDataAdapter();
            _DbDataAdapter.SelectCommand = _SelectCommand; 
        }

        public DataTable GetData(string pSQLstring)
        {

            _SelectCommand.CommandText = pSQLstring;
            DataTable dt = new DataTable();
            _DbDataAdapter.Fill(dt);
            return dt;
        }

        public void FillData(string pSQLstring, DataTable pDataTable)
        {

            _SelectCommand.CommandText = pSQLstring;
            pDataTable.Clear();
            _DbDataAdapter.Fill(pDataTable);
        }


    }
}
