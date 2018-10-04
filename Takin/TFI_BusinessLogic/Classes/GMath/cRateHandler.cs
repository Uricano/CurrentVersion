using System;
using System.Collections.Generic;
using System.Data;

// Used namespaces
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.Securities.RiskFree;
using TFI.BusinessLogic.Interfaces;
using Ness.DataAccess.Repository;
using TFI.BusinessLogic.Bootstraper;
using Cherries.Models.App;

using System.Linq;
using Entities = TFI.Entities;


namespace Cherries.TFI.BusinessLogic.GMath
{
    public class cRateHandler : IDisposable, IRateHandler
    {

        #region Data Members

        // Main variables
        private IPortfolioBL m_objPortfolio; // Portfolio data
        private IErrorHandler m_objErrorHandler; // Error handler class
        private ICollectionsHandler m_objColHandler; // Collections handler
        //private DataTable m_dtRatesRisks; // Table of rates and risks
        private bool isDisposed = false; // indicates if Dispose has already been called

        // Data variables
        //private DataTable m_dtSecurityRatesData; // Final rates calculations

        #endregion Data Members

        #region Construcors, Initialization & Destructor

        public cRateHandler(IPortfolioBL cPort)
        {
            m_objPortfolio = cPort;
            m_objErrorHandler = m_objPortfolio.cErrorLog;
            m_objColHandler = m_objPortfolio.ColHandler;

            //m_dtSecurityRatesData = getMainRatesTblStruct();
        }// constructor

        ~cRateHandler()
        { Dispose(false); }//destructor

        protected void Dispose(bool disposing)
        { // clearing class variables
            if (disposing)
            { //clean up managed resources
                m_objErrorHandler = null;
                m_objPortfolio = null;
                m_objColHandler = null;
            }
            isDisposed = true;
        }//Dispose

        public void Dispose()
        { // indicates it was NOT called by the Garbage collector
            Dispose(true);
            GC.SuppressFinalize(this); // no need to do anything, stop the finalizer from being called
        }//Dispose

        //public void clearCalcData()
        //{ // Clears relevant calculation data
        //    if (m_dtSecurityRatesData != null) m_dtSecurityRatesData.Clear();
        //    //m_objRateData.clearCalcData();
        //}//clearCalcData

        #endregion Construcors, Initialization & Destructor

        #region Methods

        //public void calcSecurityRates(ISecurities cSecsCol, cDateRange drPeriod, Boolean isRemoveDisabled)
        //{ // Calculation of the security rates (based on calculation method selected)
            
        //    try
        //    {
        //        //cQaMethods.setQaDataTable(); // Sets Datatable for work start

        //        //StringBuilder sbSecIds = new StringBuilder();
        //        //sbSecIds.AppendLine("SecurityId, Symbol, Final Rate");
        //        for (int iSecs = 0; iSecs < cSecsCol.Count; iSecs++) // Goes through securities in collection
        //        {
        //            cSecsCol[iSecs].RatesClass.setUpdatedRateData(drPeriod); // Only re-calculates if necessary

        //            //if (cProperties.isQARequested) sbSecIds.AppendLine(cSecsCol[iSecs].Properties.PortSecurityId.ToString() + ", " + cSecsCol[iSecs].Properties.SecuritySymbol + ", " +
        //            //    cSecsCol[iSecs].RatesClass.FinalRate.ToString());
        //        }

        //        if (isRemoveDisabled)
        //            cSecsCol.removeInactiveSecurities(m_objColHandler.DisabledSecs);
        //        cRiskFreeRates.isMarketChanged = false;
        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex);
        //    }
        //}//calcSecurityRates

        public List<PriceReturn> setSecuritiesPriceReturns(ISecurities cSecsCol, DateTime fromDT, DateTime toDT, string calcCurrency)
        { // Retrieves collection of pre-calculated price-returns for a given collection of securities and date-range
            // CALL PRICES FROM SP
            List<string> secIDs = getSecIDsString(cSecsCol);
            List<PriceReturn> res = new List<PriceReturn>();
            Dictionary<string, Tuple<object, NHibernate.Type.IType>> param = new Dictionary<string, Tuple<object, NHibernate.Type.IType>>();

            param.Add("security_id_list", new Tuple<object, NHibernate.Type.IType>(string.Join(",", secIDs), NHibernate.NHibernateUtil.StringClob));
            param.Add("date_start", new Tuple<object, NHibernate.Type.IType>(fromDT, NHibernate.NHibernateUtil.DateTime));
            param.Add("date_end", new Tuple<object, NHibernate.Type.IType>(toDT, NHibernate.NHibernateUtil.DateTime));
            param.Add("calc_currency", new Tuple<object, NHibernate.Type.IType>(calcCurrency, NHibernate.NHibernateUtil.String));
            IRepository repository = Resolver.Resolve<IRepository>();
            var priceReturns = repository.ExecuteSp<Entities.dbo.PriceReturn>("getPriceReturnsInDateRange", param);

            // Split prices to securities
            List<PriceReturn> finalReturns = new List<PriceReturn>();
            for (int i = 0; i < cSecsCol.Count; i++)
            { //  Split main Table of PriceReturns amongst Securities in Securities -> RateData -> RatesBacktesting (cSecsCol[i].RatesClass.RatesBacktesting)
                var securityQry = priceReturns.Where(x => x.idSecurity == cSecsCol[i].Properties.PortSecurityId);
                var secPriceReturns = securityQry.Select(x => new Cherries.Models.App.PriceReturn
                {
                    idSecurity = x.idSecurity,
                    idCurrency = x.idCurrency,
                    dtDate = x.dtDate,
                    dReturn = x.dReturn,
                    fAdjClose = x.fAdjClose,
                    fAdjClosePrevWeek = x.fAdjClosePrevWeek
                }).ToList();

                // Reverse prices
                //cSecsCol[i].RatesClass.RatesBacktesting = new List<PriceReturn>();
                //for (int iRows = ratesBackt.Count - 1; iRows >= 0; iRows--)
                //    cSecsCol[i].RatesClass.RatesBacktesting.Add(ratesBackt[iRows]);

                cSecsCol[i].RatesClass.PriceReturns = secPriceReturns;
                cSecsCol[i].RatesClass.setFinalReturn(cSecsCol[i].AvgYield);
            }
            finalReturns = AutoMapper.Mapper.Map<List<PriceReturn>>(priceReturns);

            return finalReturns;
        }//setSecuritiesPriceReturns

        private List<string> getSecIDsString(ISecurities cSecsCol)
        {// Get list of securities IDs from given securities collection
            List<string> lstFinal = new List<string>();
            for (int i = 0; i < cSecsCol.Count; i++)
                lstFinal.Add(cSecsCol[i].Properties.PortSecurityId);

            return lstFinal;
        }//GetSecIDsString


        //public void createFinalRatesTable()
        //{ // creates a global DataTable which contains the relevant securities with their rates
        //    // Inserts the currently calculated security rates to the final rates DataTable
        //    if (m_dtSecurityRatesData == null) m_dtSecurityRatesData = getMainRatesTblStruct();
        //    else m_dtSecurityRatesData.Clear(); // Resets final table

        //    for (int iSecs = 0; iSecs < m_objColHandler.ActiveSecs.Count; iSecs++)
        //        for (int iRows = 0; iRows < m_objColHandler.ActiveSecs[iSecs].RatesTable.Rows.Count; iRows++)
        //            m_dtSecurityRatesData.Rows.Add(m_objColHandler.ActiveSecs[iSecs].RatesTable.Rows[iRows]["dDate"], m_objColHandler.ActiveSecs[iSecs].Properties.PortSecurityId, m_objColHandler.ActiveSecs[iSecs].Properties.SecurityDisplay,
        //                m_objColHandler.ActiveSecs[iSecs].RatesTable.Rows[iRows][cProperties.RatesFld]);
        //}//createFinalRatesTable

        //private DataTable getMainRatesTblStruct()
        //{ // Retrieves the structure of the main rates table
        //    DataTable dtFinal = new DataTable("tbl_PriceRates");
        //    dtFinal.Columns.Add(new DataColumn("dDate", typeof(DateTime)));
        //    dtFinal.Columns.Add(new DataColumn("idSecurity", typeof(String)));
        //    dtFinal.Columns.Add(new DataColumn("strName", typeof(String)));
        //    dtFinal.Columns.Add(new DataColumn(cProperties.AdjPriceFld, typeof(double)));
        //    return dtFinal;
        //}//getMainRatesTblStruct

        //private void writeSecurityRateReturnsToCsv()
        //{ // Writes all calculated price returns to csv File
        //    StringBuilder sbRateReturns = new StringBuilder();
        //    sbRateReturns.AppendLine("SecurityId, Symbol, Date, Price, Return");
        //    for (int iRows = 0; iRows < cProperties.DataForQa.Rows.Count; iRows++)
        //        sbRateReturns.AppendLine(String.Format("{0}, {1}, {2}, {3}, {4}", cProperties.DataForQa.Rows[iRows]["idSecurity"], cProperties.DataForQa.Rows[iRows]["strSymbol"],
        //            cProperties.DataForQa.Rows[iRows]["dDate"], cProperties.DataForQa.Rows[iRows]["dPriceVal"], cProperties.DataForQa.Rows[iRows]["dReturn"]));

        //    File.WriteAllText(cProperties.qaFilePrefix +"_RateReturns.csv", sbRateReturns.ToString());
        //}//writeSecurityRateReturnsToCsv

        #endregion Calculations

        #region Rate / Risk table

        //public DataTable getSecurityRateRiskDataTable(Boolean isAllSecs, cEFHandler cEf)
        ////public DataTable getSecurityRateRiskDataTable(cEFHandler cEf)
        //{ // Retrieves a Datatable containing the average rate and risk (covariance) values for each portfolio security
        //    if (m_dtRatesRisks != null) m_dtRatesRisks.Clear();
        //    else m_dtRatesRisks = getSecRateRiskTblStruct();

        //    //for (int iSecs = 0; iSecs < m_objColHandler.ActiveSecs.Count; iSecs++) // inserts calculated rate / risk value for given security
        //    //    //if (m_objColHandler.ActiveSecs[iSecs].Weight > 0D) // Participating security
        //    //        m_dtRatesRisks.Rows.Add(m_objColHandler.ActiveSecs[iSecs].Properties.SecurityDisplay, m_objColHandler.ActiveSecs[iSecs].CovarClass.StandardDeviation,
        //    //            m_objColHandler.ActiveSecs[iSecs].RatesClass.FinalRate);

        //    int iSecPos, iCounter = (!isAllSecs) ? m_objColHandler.ActiveSecs.Count : cEf.CalculationActiveSecs.Count;
        //    for (int iSecs = 0; iSecs < iCounter; iSecs++) // inserts calculated rate / risk value for given security
        //    { // Goes through relevant securities
        //        iSecPos = (!isAllSecs) ? iSecs : cEf.CalculationActiveSecs[iSecs];
        //        if (m_objColHandler.ActiveSecs.Count > iSecPos)
        //            if ((m_objColHandler.ActiveSecs[iSecPos].Weight > 0D) || isAllSecs)
        //                //m_dtRatesRisks.Rows.Add(m_objColHandler.ActiveSecs[iSecPos].Properties.SecurityDisplay, m_objColHandler.ActiveSecs[iSecPos].CovarClass.Risk,
        //                m_dtRatesRisks.Rows.Add(m_objColHandler.ActiveSecs[iSecPos].Properties.SecurityDisplay, m_objColHandler.ActiveSecs[iSecPos].CovarClass.Risks[0],
        //                //m_dtRatesRisks.Rows.Add(m_objColHandler.ActiveSecs[iSecPos].Properties.SecurityDisplay, m_objColHandler.ActiveSecs[iSecPos].CovarClass.StandardDeviation,
        //                    m_objColHandler.ActiveSecs[iSecPos].RatesClass.FinalRate);
        //    }
        //    return m_dtRatesRisks;
        //}//getSecurityRateRiskDataTable

        private DataTable getSecRateRiskTblStruct()
        { // Retrieves the structure of the given securities table
            DataTable dtFinalVals = new DataTable();
            dtFinalVals.Columns.Add(new DataColumn("strSecName", typeof(String)));
            dtFinalVals.Columns.Add(new DataColumn("dRisk", typeof(double)));
            dtFinalVals.Columns.Add(new DataColumn("dPriceRate", typeof(double)));
            //dtFinalVals.Columns.Add(new DataColumn("dGain", typeof(double)));
            return dtFinalVals;
        }//getSecRateRiskTblStruct

        #endregion Rate / Risk table
        

        #region Properties

        //public DataTable SecurityRates
        //{
        //    get { return m_dtSecurityRatesData; }
        //    set { m_dtSecurityRatesData = value; }
        //}//SecurityRates

        //public DataTable RiskRatesTbl
        //{ get { return m_dtRatesRisks; } }//RiskRatesTbl

        #endregion Properties

    }// of class
}
