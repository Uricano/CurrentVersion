using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Cherries.TFI.BusinessLogic.DataManagement;

namespace Cherries.TFI.BusinessLogic.General
{

    public class cRepository
    {

        #region Data Members

        private DataTable m_dtSectorData = null;         // Sector dataTable
        private DataTable m_dtMarketData = null;         // Market dataTable
        private DataTable m_dtSecurityType = null;       // SecurityType dataTable

        private bool m_isHebrewOnly = false;
        #endregion

        public cRepository()
        {
            InitRepositoryClass();
        }

        private void InitRepositoryClass()
        {// Is called once in the application, from Main form

            // Get Data from Local DB
            m_dtMarketData = GetStockMarketTableData();
            m_dtSectorData = GetTableData("tblSel_Sectors");
            m_dtSecurityType = GetTableData("tblSel_SecurityType");

            
        }//InitRepositoryClass
                

        #region SQL Data handlers - Local Database
        private DataTable GetTableData(string strTblName)
        {// Fills data table from Local DB by table name
            return cDbOleConnection.FillDataTable(cSqlStatements.getTblItemsSQL(strTblName), cDbOleConnection.sqlCeConnection);
        }//GetTableData

        private DataTable GetStockMarketTableData()
        {// Fills Market data table from Local DB by table name
            return cDbOleConnection.FillDataTable(cSqlStatements.getStockMarketTable(), cDbOleConnection.sqlCeConnection);
        }//GetStockMarketTableData
        #endregion

        #region Properties

        public DataTable TblSectors
        {
            get
            {
                if (m_dtSectorData == null)
                    m_dtSectorData = GetTableData("tblSel_Sectors");
                return m_dtSectorData;
            }
        }//TblSectors

        public DataTable TblSecurityType
        {
            get
            {
                if (m_dtSecurityType == null)
                    m_dtSecurityType = GetTableData("tblSel_SecurityType");
                return m_dtSecurityType;
            }
        }//TblSecurityType

        public DataTable TblStockMarkets
        {
            get
            {
                if (m_dtMarketData == null)
                    m_dtMarketData = GetStockMarketTableData();
                return m_dtMarketData;
            }
        }//TblStockMarkets

       
        #endregion
    }
}
