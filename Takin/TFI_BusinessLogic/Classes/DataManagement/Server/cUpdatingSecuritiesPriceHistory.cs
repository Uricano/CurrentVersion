using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;




// Used namespaces
using Cherries.TFI_BusinessLogic.Protection.LicenseManagement;
using Cherries.TFI_BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI_BusinessLogic.Portfolio;
using Cherries.TFI_BusinessLogic.Securities;
using Cherries.TFI_BusinessLogic.General;
using Cherries.TFI_BusinessLogic.Tools.Settings;

namespace Cherries.TFI_BusinessLogic.DataManagement
{
    public class cUpdatingSecuritiesPriceHistory
    {

        #region Data Members

        // Main variables
        private cPortfolio m_objPortfolio; // Current Portfolio class
        private cErrorHandler m_objErrorHandler; // Error handler
        private cDbOleConnection m_objDbConnection; // Local database connection class
        private cParameters param;
                
        // Data variables
        //private DateTime m_dtDestDate = DateTime.Today.AddDays(-1); // Destination end date for bringing up to date

        // Server variables
        private TFI_CS_Shared.ResultCodes m_enumErrorCode; // Code for status in server requests
        private String m_strErrorMsg = ""; // Message from call to server

        #endregion Data Members

        #region Constructors, Initialization & Destructor

        public cUpdatingSecuritiesPriceHistory(cPortfolio cPort)
        {
           
            m_objPortfolio = cPort;
            m_objErrorHandler = m_objPortfolio.cErrorLog;
            m_objDbConnection = m_objPortfolio.OleDBConn;
            param = new cParameters(m_objErrorHandler);
            param.initParams();
        }//constructor

        #endregion Constructors, Initialization & Destructor

        #region Methods

        #region Main methods

        public void bringUpToDateAllSecurities()
        { // Brings up to date all of the securities in the database

            string lastUSD = "";
            
            DataTable dtAllSecs;
            //if(!cLicenseParams.isPrivate)
                dtAllSecs = cDbOleConnection.FillDataTable(cSqlStatements.getAllSecuritiesSortedSQL(), m_objDbConnection.dbConnection);
            //else dtAllSecs = cDbOleConnection.FillDataTable(cSqlStatements.getSecuritiesByPortIdSQL(idPortfolio), m_objDbConnection.dbConnection);


            if (dtAllSecs.Rows.Count == 0)
            {
                lastUSD = GetLastUSDprice();
                
                return;
            }

            String strSecIds = getIDListForSP(dtAllSecs);

            //string lastBringUpToDate = m_frmMain.ParameterHandler.paramByName("LastBringUpToDate");
            //if (lastBringUpToDate != "")
            //    cProperties.LastUpdate = DateTime.ParseExact(lastBringUpToDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            try
            {
                
                if (cProperties.LastUpdate < DateTime.Today)
                { // Need to bring up to date
                    DataSet dsServerData = bringUpToDateData(strSecIds, cProperties.LastUpdate, DateTime.Today.AddDays(-1));
                    setUpdatedSecurityValues(dsServerData);

                    lastUSD = dsServerData.ExtendedProperties["LastUSDprice"].ToString();
                    if (lastUSD == "")
                        cProperties.LastDollarVal = Convert.ToDouble(cGeneralFunctions.insteadValue(param.paramByName("LastUSDprice"), "390.5"));
                    else cProperties.LastDollarVal = Convert.ToDouble(lastUSD);

                    DataTable dtHistoricalData = dsServerData.Tables["tbl_Securities"]; // Don't be mistaken by the name - it's tbl_Prices

                    // Delete previous prices
                    cDbOleConnection.executeSqlSatatement(cSqlStatements.deleteDbOldPrices(DateTime.Today.AddYears(-cProperties.DatesInterval).AddDays(-1)), m_objDbConnection.dbConnection);
                    // Writes all prices after removing the concatinating previous data
                    cDbOleConnection.writePricesWithBulk(dtHistoricalData, m_objDbConnection.dbConnection, cDbOleConnection.TblPrices);
                    // Updates portfolios end dates
                    cDbOleConnection.executeSqlSatatement(cSqlStatements.updtPortfolioEndDates(cProperties.LastUpdate), m_objDbConnection.dbConnection);
                    //add code update parameter "lastBrinngUpToDate"  DateTime.Today
                    param.updateParamByValue("LastBringUpToDate", DateTime.Today.ToString("yyyy-MM-dd"));
                    if (lastUSD != "")
                        param.updateParamByValue("LastUSDprice", lastUSD);     //cProperties.LastDollarVal.ToString());
                }
                else cProperties.LastDollarVal = Convert.ToDouble(cGeneralFunctions.insteadValue(param.paramByName("LastUSDprice"), "390.5"));

            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//bringUpToDateAllSecurities

        private string GetLastUSDprice()
        { // Brings up to date the securities dynamic parameters (FAC, avgYield, stdYield)
            DataSet dsLastUSD;
            try
            {
                // In this case we take prices as is from Historical Data, so passing "-1" to stored procedure
                m_enumErrorCode = ClientBiz.cltBiz.GetLastUSDprice(out dsLastUSD, out m_strErrorMsg);

                if (m_enumErrorCode != TFI_CS_Shared.ResultCodes.ok)
                {
                    m_objErrorHandler.LogInfo(new Exception("m_strErrorMsg"));
                   
                }
                return dsLastUSD.ExtendedProperties["LastUSDprice"].ToString();
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
                throw;
            }
        }//GetLastUSDprice

        private DataSet bringUpToDateData(String SecurityIDlist, DateTime fromDate, DateTime toDate)
        { // Brings up to date the securities dynamic parameters (FAC, avgYield, stdYield)
            DataSet dsFinalData;
            try
            {
                // In this case we take prices as is from Historical Data, so passing "-1" to stored procedure
                m_enumErrorCode = ClientBiz.cltBiz.GetBringUpToDateData(SecurityIDlist, fromDate, toDate, out dsFinalData, out m_strErrorMsg);

                if (m_enumErrorCode != TFI_CS_Shared.ResultCodes.ok)
                {
                    m_objErrorHandler.LogInfo(new Exception("m_strErrorMsg"));
                    
                }
                return dsFinalData;
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
                throw;
            }
        }//bringUpToDateData

        private void setUpdatedSecurityValues(DataSet dsServerData)
        { // Updates the securities main data (FAC, avgYield, stdYield)
            DataTable dtServerSecs = dsServerData.Tables["tbl_Secs"];

            DataRow drServerSec;
            String strAvgYield, strStdYield;
            String strAvgYieldNIS, strStdYieldNIS;
            double dValueUSA, dValueNIS;
            double weightUSA, weightNIS;
            double dFAC = 1D;

            for (int iSecs = 0; iSecs < dtServerSecs.Rows.Count; iSecs++)
            {
                drServerSec = dtServerSecs.Rows[iSecs];
                if (drServerSec["dtPriceEnd"] == DBNull.Value) continue; // No data for security - skip

                strAvgYield = (drServerSec["avgYield"] == DBNull.Value) ? "" : drServerSec["avgYield"].ToString();
                strStdYield = (drServerSec["stdYield"] == DBNull.Value) ? "" : drServerSec["stdYield"].ToString();
                strAvgYieldNIS = (drServerSec["avgYieldNIS"] == DBNull.Value) ? "" : drServerSec["avgYieldNIS"].ToString();
                strStdYieldNIS = (drServerSec["stdYieldNIS"] == DBNull.Value) ? "" : drServerSec["stdYieldNIS"].ToString();
                dFAC = (drServerSec["FAC"] == DBNull.Value) ? 1D : Convert.ToDouble(drServerSec["FAC"]);
                //=======================================================================================================
                //dValue = (drServerSec["dValue"] == DBNull.Value) ? 0D : Convert.ToDouble(drServerSec["dValue"]);

                //          OR

                dValueUSA = (drServerSec["dValueUSA"] == DBNull.Value) ? 0D : Convert.ToDouble(drServerSec["dValueUSA"]);
                dValueNIS = (drServerSec["dValueNIS"] == DBNull.Value) ? 0D : Convert.ToDouble(drServerSec["dValueNIS"]);
                weightUSA = (drServerSec["WeightUSA"] == DBNull.Value) ? 0D : Convert.ToDouble(drServerSec["WeightUSA"]);
                weightNIS = (drServerSec["WeightNIS"] == DBNull.Value) ? 0D : Convert.ToDouble(drServerSec["WeightNIS"]);
                //=======================================================================================================

                //cDbOleConnection.executeSqlSatatement(cSqlStatements.updtSecurityParametersToDbSQL(dtLocalSecs.Rows[iSecs]["idSecurity"].ToString(), drServerSec["FAC"],
                cDbOleConnection.executeSqlSatatement(cSqlStatements.updtSecurityParametersToDbSQL(drServerSec["idSecurity"].ToString(), dFAC
                                                                                                   , strAvgYield, strStdYield, strAvgYieldNIS, strStdYieldNIS
                                                                                                   , dValueUSA, dValueNIS, weightUSA, weightNIS
                                                                                                   , Convert.ToDateTime(drServerSec["dtPriceEnd"]))
                                                                                                   , m_objDbConnection.dbConnection);
            }

            //cDbOleConnection.updateDataTable(cSqlStatements.getAllSecuritiesSortedSQL(), dtLocalSecs, m_objDbConnection.dbConnection);
        }//setUpdatedSecurityValues

        public double getLastDollarValue()
        { // Retrieves the last USD value from our database (after it is brought up to date)
            try
            {
                double dVal = Convert.ToDouble(cDbOleConnection.executeSqlScalarNoIDENTITY(cSqlStatements.getLastDollarValue(), m_objDbConnection.dbConnection));
                return dVal;
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
            return 390;
        }//getLastDollarValue

        #endregion Main methods

        #region Helper methods

        private String getIDListForSP(DataTable dtAllSecs)
        { // Returns list of security IDs (for import)
            String strFinal = "";
            for (int iRows = 0; iRows < dtAllSecs.Rows.Count; iRows++)
                strFinal = strFinal + string.Format(",{0}", dtAllSecs.Rows[iRows]["idSecurity"].ToString());

            if (strFinal != "")
                strFinal = strFinal.Substring(1);

            return strFinal;
        }//getIDListForSP

        private DateTime getLastDate()
        { // Retrieves the most recent date from the datebase
            try
            {
                DataTable dtPortfolios = cDbOleConnection.FillDataTable(cSqlStatements.getTblItemsSQL("tbl_Portfolios"), m_objDbConnection.dbConnection);
                if (dtPortfolios.Rows.Count == 0) return DateTime.Today; // Empty database

                return Convert.ToDateTime(dtPortfolios.Rows[0]["dtEndDate"]);
                // return Convert.ToDateTime(dtPortfolios.Rows[0]["dtPriceEnd"]);
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
            return DateTime.Today.AddDays(-1);
        }//getLastDate

        #endregion Helper methods

        #endregion Methods

    }//of class
}
