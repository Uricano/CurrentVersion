using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;


// Used namespaces
using Cherries.TFI.BusinessLogic.Portfolio;
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.Securities.RiskFree;
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.Models.dbo;
using Entities = TFI.Entities;
using Ness.DataAccess.Repository;
using System.Linq;
using AutoMapper;
using Ness.DataAccess.Fluent;
using NHibernate.Linq;
using TFI.BusinessLogic.Interfaces;

namespace Cherries.TFI.BusinessLogic.DataManagement.Prices
{
    public class cPriceData
    { // Class containing the price Data of a single security according to project

        #region Data members

        // Main variables
        private IPortfolioBL m_objPortfolio; // Current portfolio (for date completion - when modifying price type)
        private ISecurity m_objRelevantSec; // Security class relevant to current prices
        
        private IErrorHandler m_objErrorHandler; // Error handler class
        private IRepository priceRepository;
        // Data variables
        private List<Entities.dbo.Price> m_dtMainPrices; // Prices dataTable
        #endregion Data members

        #region Constructors, Initialization & Destructor

        public cPriceData(ISecurity cSec, IPortfolioBL cPort)
        {
            m_objRelevantSec = cSec;
            m_objPortfolio = cPort;
            //m_objDbConnection = m_objPortfolio.OleDBConn;
            m_objErrorHandler = m_objPortfolio.cErrorLog;
            //priceRepository = new Repository();
            initDataVariables();
        }//constructor

        #endregion Constructors, Initialization & Destructor

        #region Methods

        #region General methods

        private void initDataVariables()
        { // Initializes data variables - to check if recalculation is necessary
            if (m_dtMainPrices != null) m_dtMainPrices.Clear();
            m_dtMainPrices = null;
        }//initDataVariables

        public Boolean isNeedsRecalculation()
        { // Checks whether the current table of data needs to be reloaded
            if (m_dtMainPrices == null)
                return true;
            if (m_dtMainPrices.Count == 0) return true;
            return false;
        }//isNeedsRecalculation

        #endregion General methods

        #region Data retrieval

        public void setNewSecPrices()
        { // Sets new prices table for current security
            priceRepository.Execute(session =>
            {
                try
                {
                    initDataVariables();
                    //m_objRelevantSec.PriceTable = cDbOleConnection.FillDataTable(cSqlStatements.getSecurityPrices(new cDateRange(DateTime.Today.AddYears(-cProperties.DatesInterval), DateTime.Today.AddDays(-1)),
                    m_objRelevantSec.PriceTable = session.Query<Entities.dbo.Price>().Where(x => x.idSecurity == m_objRelevantSec.Properties.PortSecurityId).ToList();
                    setSecDateRange();
                }
                catch (Exception ex)
                {
                    m_objErrorHandler.LogInfo(ex);
                }
                finally
                {
                    setRiskfreeDailyInterest();
                }
            });
           
        }//setNewSecPrices

        private void setSecDateRange()
        { // Sets the date range of the security based on the prices datatable
            if (m_dtMainPrices.Count == 0) return;
            m_objRelevantSec.DateRange = new DataTypes.cDateRange(m_dtMainPrices[m_dtMainPrices.Count - 1].dDate,
                m_dtMainPrices[0].dDate);
        }//setSecDateRange

        private void setRiskfreeDailyInterest()
        { // Calculates the daily riskfree daily interest
            if (m_dtMainPrices.Count <= 1) return; // No viable data

            m_objRelevantSec.Analytics.OneYearDate = m_objRelevantSec.DateRange.EndDate.AddYears(-1).AddDays(1);

            DateTime dtStart = m_objRelevantSec.Analytics.OneYearDate;
           // DateTime dtEnd = (cProperties.DateSortOrder == "DESC") ? Convert.ToDateTime(m_dtMainPrices.Rows[0]["dDate"]) : Convert.ToDateTime(m_dtMainPrices.Rows[m_dtMainPrices.Rows.Count - 1]["dDate"]);
            DateTime dtEnd = m_dtMainPrices[0].dDate;
            m_objRelevantSec.Analytics.PeriodInterest = cGeneralFunctions.getPeriodYears(dtStart, dtEnd) * cRiskFreeRates.YearlyRate;
            int iPeriodDays = (dtEnd - dtStart).Days;
            double dFinalDaily = Math.Pow(1 + m_objRelevantSec.Analytics.PeriodInterest, 1 / (double)iPeriodDays) - 1;

            m_objRelevantSec.Analytics.DailyInterest = dFinalDaily;
        }//setRiskfreeDailyInterest

        //public static DataTable getPriceDataStruct()
        //{ // Returns the structue of a given dataTable
        //    DataTable dtFinal = new DataTable();
        //    dtFinal.Columns.Add(new DataColumn("dDate", typeof(DateTime)));
        //    dtFinal.Columns.Add(new DataColumn(cProperties.PricesFld, typeof(double)));
        //    return dtFinal;
        //}//getPriceDataStruct

        #endregion Data retrieval

        #region Write to DB

        //public Boolean writePricesToDB(DataTable dtMain)
        //{ return writePricesToDB(dtMain, m_objRelevantSec.Properties.PortSecurityId); }//writePricesToDB
        //private Boolean writePricesToDB(DataTable dtMain, String idSec)
        //{ // Writes the prices of the current security to DB
        //    if (dtMain.Rows.Count == 0) return false;

        //    deletePreviousSecPrices();

        //    SqlCeCommand sqlNewCommand = new SqlCeCommand("", m_objDbConnection.dbConnection);
        //    try
        //    {
        //        for (int iRows = 0; iRows < dtMain.Rows.Count; iRows++)
        //        { // Inserts each row seperately
        //            sqlNewCommand.CommandText = cSqlStatements.insertSecurityPricesToDB(dtMain.Rows[iRows], idSec);
        //            sqlNewCommand.ExecuteNonQuery();
        //        }//for
        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex);
        //    } finally {
        //        sqlNewCommand.Dispose();
        //    }
        //    return true;
        //}//writePricesToDB

        //public void writePricesWithBulk(DataTable dtMain)
        //{ // Copies prices to database
        //    if (dtMain.Rows.Count == 0) return;

        //    deletePreviousSecPrices();

        //    SqlCeBulkCopyOptions options = new SqlCeBulkCopyOptions();
        //    options = options |= SqlCeBulkCopyOptions.KeepNulls;
            
        //    using (SqlCeBulkCopy bc = new SqlCeBulkCopy(m_objDbConnection.dbConnection, options))
        //    {
        //        bc.DestinationTableName = cDbOleConnection.TblPrices;
        //        bc.WriteToServer(dtMain);
        //    }
        //}//writePricesWithBulk

        //private Boolean deletePreviousSecPrices()
        //{ // Deletes the previous prices of the current security
        //    if (m_objRelevantSec.PriceTable.Rows.Count == 0) return false;
        //    try
        //    {
        //        cDbOleConnection.executeSqlSatatement(cSqlStatements.deletePreviousSecPrices(m_objRelevantSec.DateRange, m_objRelevantSec.Properties.PortSecurityId), m_objDbConnection.dbConnection);

        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex);
        //    } 
        //    return true;
        //}//deletePreviousSecPrices

        //public static DataTable getFullDataStruct()
        //{ // Gets the data structure of a table containing all data
        //    DataTable dtFinal = new DataTable();
        //    dtFinal.Columns.Add(new DataColumn("dDate", typeof(DateTime)));
        //    dtFinal.Columns.Add(new DataColumn("fOpen", typeof(double)));
        //    dtFinal.Columns.Add(new DataColumn("fHigh", typeof(double)));
        //    dtFinal.Columns.Add(new DataColumn("fLow", typeof(double)));
        //    dtFinal.Columns.Add(new DataColumn("fClose", typeof(double)));
        //    dtFinal.Columns.Add(new DataColumn("fVolume", typeof(double)));
        //    //dtFinal.Columns.Add(new DataColumn("FAC", typeof(double)));
        //    return dtFinal;
        //}//getFullDataStruct

        //public static void deletePriceTable(int idSec, cDbOleConnection cDbConn)
        //{ // Deletes price table for a specific security
        //    cDbOleConnection.executeSqlSatatement(cSqlStatements.removeDataFromTblByIdIntSQL("tbl_Prices", "idSecurity", idSec), cDbConn.dbConnection);
        //}//deleteCurrentPriceTable

        #endregion Write to DB

        #endregion Methods

        #region Properties

        public List<Entities.dbo.Price> MainData
        {
            get { return m_dtMainPrices; }
            set { m_dtMainPrices = value; }
        }//MainData

        public IPortfolioBL Portfolio
        {
            get { return m_objPortfolio; }
            set { m_objPortfolio = value; }
        }

        #endregion Properties

    }//class
}
