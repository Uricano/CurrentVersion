using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

// Used namespaces
using Cherries.TFI.BusinessLogic.Portfolio;
using Cherries.TFI.BusinessLogic.Collections;
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.GMath.StaticMethods;
using System.Collections;
using TFI.BusinessLogic.Interfaces;
using Cherries.Models.App;
using System.Web;
using TFI.Entities.dbo;
using Ness.DataAccess.Repository;
using NHibernate.Linq;

namespace Cherries.TFI.BusinessLogic.DataManagement.Prices
{
    public class cPricesHandler : IPriceHandler
    {

        #region Data Members

        // Main variables
        private IPortfolioBL m_objPortfolio; // Portfolio class pointer
        //private cDbOleConnection m_objOleDBConn; // Main DB connection
        private ICollectionsHandler m_objColHandler; // Collections handler
        private IErrorHandler m_objErrorHandler; // Error handler class
        private IRepository _repository;
        // Data variables
        private DataTable m_dtAllSecsPrices = null; // Securities price values
        private bool isDisposed = false; // Dispose indicator

        #endregion Data Members

        #region Consturctors, Initialization & Destructor

        public cPricesHandler(IPortfolioBL cPort, IRepository repository)
        {
            m_objPortfolio = cPort;
            //m_objOleDBConn = m_objPortfolio.OleDBConn;
            m_objColHandler = m_objPortfolio.ColHandler;
            m_objErrorHandler = m_objPortfolio.cErrorLog;
            _repository = repository;
        }//constructor

        ~cPricesHandler()
        { Dispose(false); }//destructor

        public void Dispose(bool disposing)
        { // Disposing class variables
            if (disposing)
            { // Managed code
                //m_objOleDBConn = null;
                m_objErrorHandler = null;
                m_objColHandler = null;
                if (m_dtAllSecsPrices != null) m_dtAllSecsPrices.Dispose();
            }
            isDisposed = true;
        }//Dispose

        public void Dispose()
        { // Clear from memory
            Dispose(true);
            GC.SuppressFinalize(this);
        }//Dispose

        public void clearCalcData()
        { // Clears relevant calculation data
            if (m_dtAllSecsPrices != null) m_dtAllSecsPrices.Clear();
        }//clearCalcData

        #endregion Consturctors, Initialization & Destructor

        #region Methods

        #region Global datatable

        //public void setPortfolioPricesFromDB()
        //{ // Loads the prices relevant to the current portfolio
        //    try
        //    {
        //        ArrayList param = new ArrayList();
        //        cDateRange drPeriod = new cDateRange(DateTime.Today.AddYears(-cProperties.DatesInterval), DateTime.Today.AddDays(-1));

        //        for (int iSecs = 0; iSecs < m_objColHandler.Securities.Count; iSecs++)
        //        { // Loads all securities

        //            // Set adjusted price
        //            cBasicStaticCalcs.calcAdj(m_objColHandler.Securities[iSecs].PriceTable, DateTime.Today.AddDays(-1).DayOfWeek, m_objColHandler.Securities[iSecs]);
        //            //cBasicStaticCalcs.setAccumulatedCoefficientsForSecurity(m_objColHandler.Securities[iSecs].PriceTable);
        //            //cBasicStaticCalcs.calculateAdjustedPrices(m_objColHandler.Securities[iSecs].PriceTable, drPeriod);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        m_objErrorHandler.LogInfo(ex);
        //    }
        //}//setPortfolioPricesFromDB


        public List<Rate> GetPrices(string secId, string currency)
        { // Retrieves prices for securities
            List<cSecurity> lst;
            List<Price> lstPrices = null;
            List<Rate> rates = new List<Rate>();

            if (StaticData<cSecurity, ISecurities>.lst == null)
                lst = new List<cSecurity>();
            else lst = StaticData<cSecurity, ISecurities>.lst;

            var sec = lst.FirstOrDefault(x => x.Properties.PortSecurityId == secId);
            
            if (sec != null)
                lstPrices = sec.PriceTable;
            else {
                _repository.Execute(session =>
                {
                    lstPrices = session.Query<Price>().Where(x => x.idSecurity == secId).OrderByDescending(x => x.dDate).ToList();
                });
            }
            double price;

            Double basePriceforReturnCalc;
            Double currPriceforReturnCalc;

            cDateRange drCalc = new cDateRange(DateTime.Today.AddYears(-cProperties.DatesInterval), DateTime.Today.AddDays(-1));
            Double returnVaue = 0;

            DateTime baseDTforReturnCalc = lstPrices.Last().dDate;
            //TODO:CHECK FOR CURRENCY and take fcloseNIS
            basePriceforReturnCalc = lstPrices.Last().fClose.HasValue && lstPrices.Last().fClose > 0 ? lstPrices.Last().fClose.Value : lstPrices.Last().fClose.Value;

            int years = 0;
            int yearsSaved = 0;
            DateTime lastDT = lstPrices[0].dDate;

            while ((baseDTforReturnCalc.AddDays(-1)).AddYears(years) <= lastDT)
            {
                years++;
            }

            if (years > 0)
            {
                years--;
                yearsSaved = years;
            }
            int index = 0;
            for (int iRows = 0; iRows < lstPrices.Count; iRows++)
            {
                index = index + 1;
                // Replacing '.dAdjPrice' with 'fClose', but it should be rewriten to consider 'currency' (fNISClose)
                price = lstPrices[iRows].fClose.HasValue && lstPrices[iRows].fClose > 0 ? lstPrices[iRows].fClose.Value : lstPrices[iRows].fClose.Value;
                currPriceforReturnCalc = price;
                if (index == 7 || (years > 0 && (baseDTforReturnCalc.AddDays(-1)).AddYears(years).ToOADate() == lstPrices[iRows].dDate.ToOADate()))
                {
                    string label = "";
                    if (years > 0 && (baseDTforReturnCalc.AddDays(-1)).AddYears(years).ToOADate() == lstPrices[iRows].dDate.ToOADate())
                    {
                        returnVaue = (currPriceforReturnCalc / basePriceforReturnCalc - 1) * 100;
                        label = string.Format("{0} years return:{2}{1:#,##0.00} %", (years).ToString(), returnVaue, "");
                        years--;
                    }
                    rates.Add(new Rate { Date = lstPrices[iRows].dDate, RateVal = price, Tooltip = string.Format("Date : {0:dd/MM/yy} Price: {1:#,##0.00}", lstPrices[iRows].dDate, price), Label = label });
                    index = 0;
                }
            }
            return rates.OrderBy(o => o.Date).ToList();
        }

        public Rate GetPrice(string secId, string currency, DateTime date)
        {
            List<cSecurity> lst;
            Rate rate = new Rate();
            if (StaticData<cSecurity, ISecurities>.lst == null)
            {
                lst = new List<cSecurity>();
            }
            else
                lst = StaticData<cSecurity, ISecurities>.lst;

            cSecurity sec = lst.FirstOrDefault(x => x.Properties.PortSecurityId == secId);
            Price price = null;
            if (sec == null)
            {
                _repository.Execute(session =>
                {
                    price = session.Query<Price>().Where(x => x.idSecurity == secId && x.dDate <= date).FirstOrDefault();
                });
            }
            else {
                price = sec.PriceTable.Where(x => x.dDate <= date).FirstOrDefault();
                
            }
            if (price != null) rate = new Rate { Date = price.dDate, RateVal = (currency == "9001" ? price.fClose : price.fNISClose) };
            return rate;
        }
        //public void calcAdjusted()
        //{ // Calculates adjusted prices for all securities
        //    try
        //    {
        //        for (int iSecs = 0; iSecs < m_objColHandler.Securities.Count; iSecs++)
        //        { // Goes through securities
        //            DataTable priceTable = m_objColHandler.Securities[iSecs].PriceTable;

        //            DataColumn fAdjCol = new DataColumn("fAdjClose", System.Type.GetType("System.Double"));
        //            priceTable.Columns.Add(fAdjCol);

        //            double facAccum = 1.0;
        //            for (int iPrice = priceTable.Rows.Count - 1; iPrice >= 0; iPrice--)
        //            {
        //                DataRow priceRow = priceTable.Rows[iPrice];
        //                facAccum *= Convert.ToDouble(priceRow["FAC"]);
        //                priceRow["fAdjClose"] = facAccum * Convert.ToDouble(priceRow["fClose"]);
        //                //Console.WriteLine(Convert.ToDateTime(priceRow["dDate"]) + " " + Convert.ToDouble(priceRow["fClose"]) + " " + Convert.ToDouble(priceRow["fAdjClose"]) + " " + Convert.ToDouble(priceRow["FAC"]));
        //            }
        //        }
        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex);
        //    }
        //}//calcAdjusted

        //public DataTable calcWeeklyChangeForSecr(DataTable priceTable)
        //{
        //    DataTable weeklyChangesTable = new DataTable();
        //    DataColumn weeklyChangesTable_date = new DataColumn("date", System.Type.GetType("System.DateTime"));
        //    weeklyChangesTable.Columns.Add(weeklyChangesTable_date);
        //    DataColumn weeklyChangesTable_change = new DataColumn("change", System.Type.GetType("System.Double"));
        //    weeklyChangesTable.Columns.Add(weeklyChangesTable_change);

        //    DataRow priceRow = priceTable.Rows[0];
        //    double price = Convert.ToDouble(priceRow["fAdjClose"]);
        //    DateTime priceDate = Convert.ToDateTime(priceRow["dDate"]).Date;
        //    DateTime dateToFind = priceDate;

        //    for (int iDailyPrice = 0; iDailyPrice < priceTable.Rows.Count; )
        //    {
        //        double price_o = price;
        //        DateTime priceDate_o = priceDate;

        //        dateToFind = dateToFind.AddDays(-7);
        //        for (; ; )
        //        {
        //            if (++iDailyPrice >= priceTable.Rows.Count)
        //                break;
        //            priceRow = priceTable.Rows[iDailyPrice];
        //            price = Convert.ToDouble(priceRow["fAdjClose"]);
        //            priceDate = Convert.ToDateTime(priceRow["dDate"]).Date;
        //            if (priceDate.CompareTo(dateToFind) <= 0)
        //            {
        //                double weeklyChange = price_o / price - 1.0;
        //                //Console.WriteLine(priceDate_o + " " + priceDate + " " + price_o + " " + price + " " + weeklyChange+" "+priceDate_o.DayOfWeek);
        //                DataRow weeklyChangesTable_row = weeklyChangesTable.NewRow();
        //                weeklyChangesTable_row["date"] = priceDate_o;
        //                weeklyChangesTable_row["change"] = weeklyChange;
        //                break;
        //            }
        //        }
        //    }

        //    return weeklyChangesTable;
        //}

        //public void setPortfolioAdjustedPrices()
        //{
        //    DateTime dtStartDate = DateTime.Today.AddYears(-cProperties.DatesInterval);
        //    DateTime dtEndDate = DateTime.Today.AddDays(-1);

        //    try
        //    {
        //        for (int iSecs = 0; iSecs < m_objColHandler.Securities.Count; iSecs++)  // Loads all securities
        //        {
        //            m_objColHandler.Securities[iSecs].PriceTable = cDbOleConnection.FillDataTable(cSqlStatements.getSecurityAdjustedPrices(new cDateRange(dtStartDate, dtEndDate),
        //                m_objColHandler.Securities[iSecs].Properties.PortSecurityId), m_objOleDBConn.dbConnection);

        //            DataColumn facAccumCol = new DataColumn("FACAccumulated");
        //            facAccumCol.DataType = System.Type.GetType("System.Double");
        //            m_objColHandler.Securities[iSecs].PriceTable.Columns.Add(facAccumCol);

        //            double facAccum = 1.0;
        //            for (int i = 0; i < m_objColHandler.Securities[iSecs].PriceTable.Rows.Count; i++)
        //            {
        //                DataRow row = m_objColHandler.Securities[iSecs].PriceTable.Rows[i];
        //                row["fClose"] = Convert.ToDouble(row["fClose"]) * Convert.ToDouble(row["FAC"]);
        //                row["FACAccumulated"] = facAccum * Convert.ToDouble(row["FAC"]);
        //                facAccum = Convert.ToDouble(row["FACAccumulated"]);
        //            }
        //        }
        //    } 
        //    catch (Exception ex) 
        //    {
        //        m_objErrorHandler.LogInfo(ex);
        //    }

        //}//setPortfolioAdjustedPrices

        //public double getPortfolioOneDayReturn(DateTime dateOfReturn)
        //{
        //    double rtn = 0.0;
        //    try
        //    {
        //        for (int iSecs = 0; iSecs < m_objColHandler.Securities.Count; iSecs++)   //all securities
        //        {
        //            double secWeigth = m_objColHandler.Securities[iSecs].Weight;
        //            if (secWeigth != 0.0)
        //            {
        //                DateTime dateOfReturnCorrected;
        //                DateTime prevDate;
        //                double priceOnDateOfReturn = getLastValidDatePrice(m_objColHandler.Securities[iSecs].PriceTable, dateOfReturn, out dateOfReturnCorrected);
        //                double priceOnPrevDate = getLastValidDatePrice(m_objColHandler.Securities[iSecs].PriceTable, dateOfReturnCorrected.AddDays(-1), out prevDate);
        //                rtn += ((priceOnDateOfReturn / priceOnPrevDate - 1.0) * secWeigth);
        //            }
        //        }
        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex);
        //    }
        //    return rtn;
        //} //getPortfolioOneDayReturn

        //private double getLastValidDatePrice(DataTable priceTable, DateTime specifiedDate, out DateTime dateCorrected)
        //{ 
        //    for (int iPrice = 0; iPrice < priceTable.Rows.Count; iPrice++)
        //    {
        //        DataRow row = priceTable.Rows[iPrice];
        //        DateTime dateWrk = Convert.ToDateTime(row["dDate"]).Date;
        //        bool isHoliday = Convert.ToBoolean(row["isHoliday"]);
        //        if ((dateWrk.CompareTo(specifiedDate.Date) <= 0) && (!isHoliday))
        //        {
        //            dateCorrected = dateWrk;
        //            return Convert.ToDouble(row["fClose"]);
        //        }
        //    }
        //    throw new Exception("price for specified date not found");
        //}//getLastValidDatePrice

        //private cDateRange getSecDateRange(DataTable dtPrices)
        //{ // Deduces the security's date range
        //    DateTime dtStart = ((dtPrices != null) && (dtPrices.Rows.Count > 0)) ? Convert.ToDateTime(dtPrices.Rows[0]["dDate"]) : DateTime.Today.AddYears(-cProperties.DatesInterval);
        //    DateTime dtEnd = ((dtPrices != null) && (dtPrices.Rows.Count > 0)) ? Convert.ToDateTime(dtPrices.Rows[dtPrices.Rows.Count - 1]["dDate"]) : DateTime.Today.AddDays(-1);
        //    return new cDateRange(dtStart, dtEnd);
        //}//getSecDateRange

        #endregion Global datatable

        #endregion Methods

        #region Properties

        public DataTable SecPrices
        {
            get { return m_dtAllSecsPrices; }
            set { m_dtAllSecsPrices = value; }
        }//SecurityRates

        public cCollectionsHandler ColHandler
        { set { m_objColHandler = value; } }//ColHandler

        #endregion Properties

    }//of class
}
