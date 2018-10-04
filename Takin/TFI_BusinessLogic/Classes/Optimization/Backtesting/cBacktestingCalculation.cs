using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

// Used namespaces
using Cherries.TFI.BusinessLogic.Portfolio;
using Cherries.TFI.BusinessLogic.Collections;
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.DataManagement;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.DataManagement.StaticMethods;
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.GMath.StaticMethods;
using Cherries.TFI.BusinessLogic.Tools;
using Cherries.Models.dbo;
using Cherries.Models.ViewModel;
using Cherries.Models.App;
using TFI.BusinessLogic.Interfaces;
using Entities = TFI.Entities;
using Ness.DataAccess.Repository;

namespace Cherries.TFI.BusinessLogic.Optimization.Backtesting
{
    public class cBacktestingCalculation : IBacktestingCalculation
    {

        #region Data members

        // Project variables
        private IPortfolioBL m_objPortfolio; // Current Portfolio class
        private ICollectionsHandler m_objColHandler; // Collection handler
        //private cEFHandler m_objEfHandler; // Markowitz calculation handler
        private IOptimizationResults m_objEfHandler; // Markowitz calculation handler
        private IErrorHandler m_objErrorHandler; // Error handler

        // Calculation parameters
        private double m_dPortEquity = 0D; // Portfolio's equity
        private cDateRange m_drDateRange = new cDateRange(DateTime.Today, DateTime.Today); // Date range for backtesting
        private const int m_IntervalDays = 7; // Datapoints interval (per days)
        private int m_countPortSecs = 0; // Number of securities for calculated portfolio

        // Data variables
        private DataTable m_dtAllVals; // Original portfolio datatable
        private DataTable m_dtSecuritiesWeights = null; // Table of securities quantities (weights) for the current portfolio
        private DataTable m_dtSecsPricesInDateRange = null; // Table of securities prices in date range with days interval
        private DataTable m_dtBMPricesInDateRange = null; // Table of benchmark prices in date range with days interval
        private List<BenchMarkResult> m_dtReturnByDate = new List<BenchMarkResult>(); // Table of Portfolio and Benchmark Returns in date range with days interval, ordered by Date
        private List<double> m_colLastPricesStart = new List<double>(); // Collection of last prices for securities (for profit calculation - Start of period)
        private List<double> m_colLastPricesEnd = new List<double>(); // Collection of last prices for securities (for profit calculation - End of period)
        private List<double> m_colLastPricesCurrent = new List<double>(); // Collection of last prices for securities (for profit calculation - Current date)
        private String m_strBenchID = "0";      // Benchmark ID
        private ISecurity m_currBenchmarkSec;   // Benchmark Security
        private double m_dCurrPortRiskVal = 0D; // Current risk value
        private double m_dFinalProfit = 0D; // Portfolio's profit
        private double m_dPortfolioReturn = 0D; // Portfolio's return
        private double m_dBMarkReturn = 0D; // Benchmark's return
        private double m_dBenchmarkRisk = 0; // Risk of selected benchmark
        //DataTable m_dtBMrates;              // Benchmark rate table for drawing GRAPH

        // General variables
        private String m_Currency;      // Currency type
        private String m_CurrencySign;  // Currency sign (for display)

        private double m_AdjCoeff;
        private DateTime constMinDT;    // = new DateTime(1900, 01, 01); // constant date, indicating that current date is the end date of period
        private double m_dClosestPortRiskVal;

        #region Cloud
        private IRepository repository;// = new Repository();
        #endregion Cloud

        #endregion Data members

        #region Consturctors, Initialization & Destructor

        public cBacktestingCalculation()
        {
           
        }//constructor

        private void SetData(IPortfolioBL cPort, DateTime dtStartDate, DateTime dtEndDate, double dEquity, String idBenchmark)
        {
            m_objPortfolio = cPort;
            m_objColHandler = m_objPortfolio.ColHandler;
            m_objEfHandler = m_objPortfolio.Classes.Optimizer;
            m_objErrorHandler = m_objPortfolio.cErrorLog;

            try
            {
                m_strBenchID = idBenchmark;
                m_dPortEquity = dEquity;
                m_drDateRange = new cDateRange(dtStartDate, dtEndDate); // For event purposes

                initMainVars();

            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//SetData

        private void initMainVars()
        { // Initializes main variables in class
            m_Currency = cGeneralFunctions.getCurrencyName(m_objPortfolio.Details.CalcCurrency, out m_CurrencySign, out m_AdjCoeff);
            //m_AdjCoeff = (m_objPortfolio.Details.CalcCurrency == "9999") ? 100.0 : 1;    // Convert agorot into shekels
            constMinDT = new DateTime(1900, 01, 01);
        }//initMainVars

        #endregion Consturctors, Initialization & Destructor

        #region Methods

        #region Calculation Methods

        #region Main calculation

        public BackTestingViewModel calculateBacktesting(IPortfolioBL cPort, DateTime dtStartDate, DateTime dtEndDate, double dEquity, String idBenchmark)
        { // Main backtesting calculation
            SetData(cPort, dtStartDate, dtEndDate, dEquity, idBenchmark);
            BackTestingViewModel vm = new BackTestingViewModel();
            try
            {
                setDisabledSecsForBacktesting(m_drDateRange.StartDate);

                m_objEfHandler.calculateEfficientFrontier(m_objEfHandler.SecuritiesCol, m_objColHandler.Benchmarks, new cDateRange(Convert.ToDateTime(m_drDateRange.StartDate).AddYears(-1), Convert.ToDateTime(m_drDateRange.StartDate)), true, false);
                if (m_objEfHandler.isSuccessful)
                { // If Successful - refreshes control
                    //m_objEfHandler.Weights.setOptimalSecsUniverse();

                    m_colLastPricesEnd = getLastPricesCol(m_drDateRange.EndDate);
                    m_colLastPricesStart = getLastPricesCol(m_drDateRange.StartDate);

                    //calculateBenchmarkRisk();
                    calculateBenchmarkRiskAndRefreshTables();
                    cProperties.NumOptimizations--; // Used one license optimization
                    vm = AutoMapper.Mapper.Map<BackTestingViewModel>(this);
                }
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
            return vm;
        }//calculateBacktesting

        private void calculatePortfolioAndBmarkReturnsForPeriod()
        {
            try
            {

                // Benchmark security rates calculations
                m_currBenchmarkSec.RatesClass.calculateBacktestingRates(m_drDateRange);

                // Portfolio securities rates calculations
                for (int i = 0; i < m_objEfHandler.SecuritiesCol.Count; i++)
                {
                    if (m_objEfHandler.SecuritiesCol[i].Weight > 0D)
                        m_objEfHandler.SecuritiesCol[i].RatesClass.calculateBacktestingRates(m_drDateRange);
                    else
                        m_objEfHandler.SecuritiesCol[i].RatesClass.RatesBacktesting = null;
                }
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//calculateBacktesting

        private void setDisabledSecsForBacktesting(DateTime dtEndDate)
        { // Sets the disabled securities that don't meet the date-range requirement
            bool doDisableSec = false;
            int cnt;
            m_objColHandler.DisabledSecs.Clear();
            for (int iSecs = 0; iSecs < m_objEfHandler.SecuritiesCol.Count; iSecs++)
            {
                doDisableSec = false;
                if (cGeneralFunctions.isBelowNumberOfMonths(new cDateRange(m_objEfHandler.SecuritiesCol[iSecs].DateRange.StartDate, dtEndDate), cProperties.MinMonthsData))
                    doDisableSec = true;

                // Dissable sec if it does not have prices on all the tested period
                if (Convert.ToDateTime(m_objEfHandler.SecuritiesCol[iSecs].PriceTable[0].dDate) < m_drDateRange.EndDate.AddDays(-1))
                    doDisableSec = true;

                cnt = m_objEfHandler.SecuritiesCol[iSecs].PriceTable.Count;
                if (Convert.ToDateTime(m_objEfHandler.SecuritiesCol[iSecs].PriceTable[cnt - 1].dDate) > m_drDateRange.StartDate)
                    doDisableSec = true;

                if (doDisableSec)
                    m_objEfHandler.SecuritiesCol[iSecs].disableCurrentSecurity();

            }
        }//setDisabledSecsForBacktesting

        public void calculateBenchmarkRiskAndRefreshTables()
        {// Calculates  Benchmark risk based on Standard deviation of Benchmark yields 
            try
            {
                double currRisk = setBenchmarkAndGetRiskByID(m_strBenchID);
                refreshTablesByRiskValue(currRisk);
            }
            catch (Exception ex)
            {
                m_objErrorHandler.LogInfo(ex);
            }
        }//calculateBenchmarkRisk
        private void calculateBenchmarkRisk()
        {// Calculates  Benchmark risk based on Standard deviation of Benchmark yields 
            try
            {
                ////=====================  Old application calculations  =========================
                //DateTime firstDate = Convert.ToDateTime(m_dtReturnByDate.Rows[0]["StartDate"]);
                //double stdDev = Convert.ToDouble(m_dtReturnByDate.Compute("StDev(IndexReturnStdDev)", string.Format("StartDate > #{0}#", firstDate.ToString("yyyy/MM/dd")))) / 100.0;
                //m_dBenchmarkRisk = cClientStaticMethods.NormalRisk(stdDev * Math.Sqrt(52)) * 100;    //Laura - new Benchmark risk calculations (25/02/16)
                ////==============================================================================

                m_currBenchmarkSec = m_objColHandler.Benchmarks.getSecurityById(m_strBenchID);
                m_dBenchmarkRisk = m_currBenchmarkSec.CovarClass.StandardDeviation * 100;   

                // Set risk spin edit control value
                double min = Convert.ToDouble(m_objEfHandler.RiskRange.Y);
                double max = Convert.ToDouble(m_objEfHandler.RiskRange.X);
                double currRisk;
 
                if (m_dBenchmarkRisk < min)
                    currRisk = Convert.ToDouble(min);
                else if (m_dBenchmarkRisk > max)
                    currRisk = Convert.ToDouble(max);
                else
                    currRisk = Convert.ToDouble(m_dBenchmarkRisk);

                // Per Nissim's request on 01/11/15, take next risk level
                currRisk = (Convert.ToDouble(currRisk) * 1.05) / 100.0;

                int iRowInd = m_objEfHandler.getClosestRiskValIndex(currRisk);
                //int iRowInd = m_objEfHandler.getClosestBiggerRiskValIndex(currRisk);    // calling new function, which brings always biggest closest

                //m_dCurrPortRiskVal = m_objEfHandler.CalculatedRisks[iRowInd];           // Set new risk value
                
                refreshTablesPos(iRowInd);
            }
            catch (Exception ex)
            {
                m_objErrorHandler.LogInfo(ex);
            }
        }//calculateBenchmarkRisk
        public double setBenchmarkAndGetRiskByID(string p_strBenchID)
        {// Sets Benchmark ID
            m_strBenchID = p_strBenchID;
            m_currBenchmarkSec = m_objColHandler.Benchmarks.getSecurityById(m_strBenchID);
            m_dBenchmarkRisk = m_currBenchmarkSec.CovarClass.StandardDeviation * 100;

            // Set risk spin edit control value
            double min = Convert.ToDouble(m_objEfHandler.RiskRange.Y);
            double max = Convert.ToDouble(m_objEfHandler.RiskRange.X);
            double currRisk;

            if (m_dBenchmarkRisk < min)
                currRisk = Convert.ToDouble(min);
            else if (m_dBenchmarkRisk > max)
                currRisk = Convert.ToDouble(max);
            else
                currRisk = Convert.ToDouble(m_dBenchmarkRisk);

            // Per Nissim's request on 01/11/15, take next risk level
            currRisk = (Convert.ToDouble(currRisk) * 1.05) / 100.0;
            // Save closest to BM risk portfolio risk level
            int iRowInd = m_objEfHandler.getClosestRiskValIndex(currRisk);
            m_dClosestPortRiskVal = m_objEfHandler.Portfolios[iRowInd].Risk;
            return currRisk;
        }

        #endregion Main calculation

        #region Portfolio & Benchmark calculations - NOT USED!!!!!!

        //private void calculatePortfolioReturnsByDates()
        //{ // Not used NOW !!!!!!!!!!!!!!!
        //  // Calculates and fills in 4 columns 'PortAmount', 'PortReturn', 'IndexAmount', 'IndexReturn' in the table for the chart
        //    
        //    if (m_dtSecsPricesInDateRange == null || m_dtSecsPricesInDateRange.Rows.Count == 0) return;
        //    try
        //    {
        //        // Variables
        //        Boolean isSkipCurrentDateCalculations = false, isFirstAmountsInitialized = false;
        //        double dCurrDateIndexAmt = 0, dPortAmount = 0, dSecQuantity;
        //        double firstIndexAmount = -99999, firstPortAmount = -99999, prevIndexAmount = -99999;
        //        DataRow drSecs = null;
        //        int iSecsCount = 0;
        //        string currSecurityID;

        //        // Starting Date
        //        DateTime currDate = Convert.ToDateTime(m_dtSecsPricesInDateRange.Rows[0]["dDate"]);
        //        DataRow[] arrRows = m_dtBMPricesInDateRange.Select(string.Format("dDate = #{0}#", currDate.ToString("yyyy/MM/dd")));

        //        // Starting Price
        //        if (arrRows.Length == 0) isSkipCurrentDateCalculations = true;
        //        else { isSkipCurrentDateCalculations = false; dCurrDateIndexAmt = Convert.ToDouble(arrRows[0][m_strCloseFld]); }

        //        // In order not to repeat same code for last date in the table, making one more entry in the 'FOR' loop to write last date's info
        //        // Variable 'lastIteration' is indicating that case
        //        m_dtReturnByDate.Rows.Clear();
        //        for (int iRows = 0; iRows <= m_dtSecsPricesInDateRange.Rows.Count; iRows++)
        //        { // Goes through all prices
        //            if (iRows != m_dtSecsPricesInDateRange.Rows.Count) drSecs = m_dtSecsPricesInDateRange.Rows[iRows];

        //            if (iRows == m_dtSecsPricesInDateRange.Rows.Count || currDate != Convert.ToDateTime(drSecs["dDate"]))
        //            { // Date change / Last iteration
        //                if (!isSkipCurrentDateCalculations && iSecsCount == m_countPortSecs)
        //                    m_dtReturnByDate.Rows.Add(registerPortfolioDateValues(currDate, dPortAmount, dCurrDateIndexAmt, firstIndexAmount, firstPortAmount, prevIndexAmount, isFirstAmountsInitialized));

        //                if (iRows != m_dtSecsPricesInDateRange.Rows.Count)
        //                    advanceCurrDate(ref currDate, drSecs, ref arrRows, ref isSkipCurrentDateCalculations, ref dCurrDateIndexAmt, ref dPortAmount, ref iSecsCount);
        //            }

        //            if ((iRows != m_dtSecsPricesInDateRange.Rows.Count) && !isSkipCurrentDateCalculations)
        //            { // Calculate for one security
        //                currSecurityID = drSecs["idSecurity"].ToString();
        //                if (getSecQuantity(currSecurityID, out dSecQuantity))
        //                {
        //                    dPortAmount += dSecQuantity * Convert.ToDouble(drSecs[m_strCloseFld]);  // Price is in agorot, so we have to divide by 100
        //                    iSecsCount++;
        //                }
        //            }
        //        }//main for

        //        m_dtReturnByDate.AcceptChanges();
        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex);
        //    }
        //}//calculatePortfolioReturnsByDates

        //private DataRow registerPortfolioDateValues(DateTime currDate, double dPortAmount, double dCurrDateIndexAmt, double firstIndexAmount, double firstPortAmount, double prevIndexAmount, bool IsFirstAmountsInitialized)
        //{ // NOT USED NOW!!!!!!!!!!!!!
        //  // Registers the current portfolio values (for a specific date)
        //    DataRow dr = m_dtReturnByDate.NewRow();
        //    dr["StartDate"] = currDate;
        //    dr["PortAmount"] = dPortAmount;
        //    dr["IndexAmount"] = dCurrDateIndexAmt;

        //    if (!IsFirstAmountsInitialized)
        //    { // Assign first port and index amounts (only once)
        //        firstIndexAmount = Convert.ToDouble(dr["IndexAmount"]);
        //        firstPortAmount = Convert.ToDouble(dr["PortAmount"]);
        //        IsFirstAmountsInitialized = true;
        //    }

        //    dr["IndexReturn"] = (Convert.ToDouble(dr["IndexAmount"]) / firstIndexAmount - 1) * 100;
        //    dr["PortReturn"] = (Convert.ToDouble(dr["PortAmount"]) / firstPortAmount - 1) * 100;

        //    // ========== Calculate another column for Benchmark risk calculations later on ==============================
        //    if (m_dtReturnByDate.Rows.Count == 0)
        //        prevIndexAmount = Convert.ToDouble(dr["IndexAmount"]);
        //    else
        //        prevIndexAmount = Convert.ToDouble(m_dtReturnByDate.Rows[m_dtReturnByDate.Rows.Count - 1]["IndexAmount"]);

        //    dr["IndexReturnStdDev"] = (Convert.ToDouble(dr["IndexAmount"]) / prevIndexAmount - 1) * 100;
        //    // ===========================================================================================================

        //    return dr;
        //}//registerPortfolioDateValues

        //private void advanceCurrDate(ref DateTime currDate, DataRow drSecs, ref DataRow[] arrRows, ref Boolean isSkipCurrentDateCalculations, ref double dCurrDateIndexAmt, ref double dPortAmount, ref int secsCount)
        //{ // Replace currDate with a new date
        //    currDate = Convert.ToDateTime(drSecs["dDate"]);
        //    arrRows = m_dtBMPricesInDateRange.Select(string.Format("dDate = #{0}#", currDate.ToString("yyyy/MM/dd")));
        //    if (arrRows.Length == 0)
        //        isSkipCurrentDateCalculations = true;
        //    else
        //    {
        //        isSkipCurrentDateCalculations = false;
        //        dCurrDateIndexAmt = Convert.ToDouble(arrRows[0][m_strCloseFld]);
        //    }
        //    // Reset portfolio amount and securities counter
        //    dPortAmount = 0;
        //    secsCount = 0;
        //}//advanceCurrDate

        //private bool getSecQuantity(string currSecurityID, out double secQuantity)
        //{ // Extracts security Quantity from the table by SecurityID
        //    secQuantity = 0;
        //    try
        //    {
        //        DataRow[] arrWeight = m_dtSecuritiesWeights.Select(string.Format("securityId = '{0}'", currSecurityID));
        //        if (arrWeight.Length > 0)
        //        {
        //            secQuantity = Convert.ToDouble(arrWeight[0]["Quantity"]); return true;
        //        } else return false;

        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex);
        //        return false;
        //    }
        //}//getSecQuantity

        #endregion Portfolio & Benchmark calculations

        #endregion Calculation Methods

        #region Risk Modification

        public void refreshTablesByRiskValue(double dRiskVal)
        { // Refreshes Grid tables with the current portfolio risk value
            if (m_objEfHandler.Portfolios.Count == 0) return; // No data
            try
            {
                int iRowInd = m_objEfHandler.getClosestRiskValIndex(dRiskVal); 
                refreshTablesPos(iRowInd);

            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//refreshTablesByRiskValue

        private void refreshTablesPos(int iPos)
        { // Refreshes graphs with a specified portfolio position

            try
            {
                m_dCurrPortRiskVal = m_objEfHandler.Portfolios[iPos].Risk; // Set new risk value
                m_dPortfolioReturn = m_objEfHandler.Portfolios[iPos].Return;
                CurrPortRateToRisk = m_objEfHandler.Portfolios[iPos].RateToRisk;
                CurrPortDiversification = m_objEfHandler.Portfolios[iPos].Diversification;
                m_objEfHandler.PortNumA = iPos;
                m_objEfHandler.setCollectionsDataForRiskLevel(iPos);

                fillOriginalPortfolioDatatable();

                ////===========  Previous version with prices from local DB and calculations ==============
                //if (!getPricesDataTablesInDaterange()) return; // Get 3 tables from database
                //fillSecuritiesWeightsDT(); // Fill in weight, amount and quantity of all securities
                //calculatePortfolioReturnsByDates(); // Calculate Portfolio and Index returns
                ////=======================================================================================


                //===========  New version with previously calculated data in COLLECTIONS =================
                // TODO: CHECK if next function is needed, I think - NO!!!
                calculatePortfolioAndBmarkReturnsForPeriod();

                fillPortAndBMarksReturnsTable(); // Fill in IndexReturn and PortReturn for GRAPH points
                //=========================================================================================

            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }

        private void fillPortAndBMarksReturnsTable()
        {// Builds table with BMark return value, portfolio return value for the period, used for drawing backtesting GRAPH

            try
            {
                bool IsSkipCurrentDateCalculations;
                DateTime stepDate;
                double IndexReturn, IndexAmount = 0, cumulIndexReturn = 0;
                double PortReturn, PortAmount = 0, cumulPortReturn = 0;
                int secsCount = 0;
                Rate dr;
                List<Entities.dbo.Price> arrRows;
                double BmQty = 1;


                m_dtReturnByDate = new List<BenchMarkResult>();
                SetSecuritiesWithWeightNumber();

                stepDate = Convert.ToDateTime(m_drDateRange.StartDate);
                arrRows = m_currBenchmarkSec.PriceTable.Where(x => x.dDate == stepDate).ToList();
                if (arrRows.Count > 0)
                {
                    IndexAmount = Convert.ToDouble(arrRows[0].fClose);      //LR:close price patch
                    BmQty = m_dPortEquity / IndexAmount * m_AdjCoeff;  // agorot to shekels    //double division is multiplication
                }


                for (int i = 0; i < m_currBenchmarkSec.RatesClass.RatesBacktesting.Count; i++)
                {
                    dr = m_currBenchmarkSec.RatesClass.RatesBacktesting[i];
                    stepDate = Convert.ToDateTime(dr.Date);
                    IndexReturn = Convert.ToDouble(dr.RateVal);
                    arrRows = m_currBenchmarkSec.PriceTable.Where(x => x.dDate == stepDate).ToList();
                    if (arrRows.Count == 0)
                        IsSkipCurrentDateCalculations = true;
                    else
                    {
                        IsSkipCurrentDateCalculations = false;
                        IndexAmount = Convert.ToDouble(m_currBenchmarkSec.IdCurrency == "9001" ? arrRows[0].fClose : arrRows[0].fNISClose);  //LR:close price patch
                    }

                    if (!IsSkipCurrentDateCalculations)
                    {
                        PortReturn = GetPortfolioReturnOnDate(stepDate, out secsCount, out PortAmount);

                        if (secsCount == m_countPortSecs)
                        {
                            //cumulIndexReturn += IndexReturn;
                            //cumulPortReturn += PortReturn;

                            cumulPortReturn = (PortAmount - m_dPortEquity) / m_dPortEquity;
                            cumulIndexReturn = ((IndexAmount / m_AdjCoeff) * BmQty - m_dPortEquity) / m_dPortEquity;  // agorot to shekels    //double division is multiplication

                            m_dtReturnByDate.Add(new BenchMarkResult { StartDate = stepDate, PortAmount = PortAmount, PortReturn = cumulPortReturn, IndexAmount = IndexAmount, IndexReturn = cumulIndexReturn });
                        }
                    }
                }

                // Because weekly division starts from the end of period till the beginning, we might miss the beginning date,
                // so adding it manualy
                stepDate = Convert.ToDateTime(m_drDateRange.StartDate);
                IndexReturn = 0;
                arrRows = m_currBenchmarkSec.PriceTable.Where(x => x.dDate == stepDate).ToList();
                if (arrRows.Count == 0)
                    IsSkipCurrentDateCalculations = true;
                else
                {
                    IsSkipCurrentDateCalculations = false;
                    IndexAmount = Convert.ToDouble(m_currBenchmarkSec.IdCurrency == "9001" ? arrRows[0].fClose : arrRows[0].fNISClose); //LR:close price patch
                }

                if (!IsSkipCurrentDateCalculations)
                {
                    PortAmount = GetPortfolioValueOnStartDate(out secsCount);

                    if (secsCount == m_countPortSecs)
                    {
                        var res = new BenchMarkResult();
                        res.StartDate = stepDate;
                        res.PortAmount = PortAmount;
                        res.PortReturn = 0;
                        res.IndexAmount = IndexAmount;
                        res.IndexReturn = 0;
                        m_dtReturnByDate.Insert(0, res);
                    }
                }
            }
            catch (Exception ex)
            {
                m_objErrorHandler.LogInfo(ex);
            }
        }//fillPortAndBMarksReturnsTable

        private double GetPortfolioValueOnStartDate(out int secsCount)
        {// Calculates portfolio value on Start date

            secsCount = Convert.ToInt32(m_dtAllVals.Compute("COUNT(dWeight)", "dWeight > 0"));  //m_dtAllVals.Rows.Count;
            try
            {
                return Convert.ToDouble(m_dtAllVals.Compute("SUM(dValNew)", "")); //Value on 'Start date' of date range
            }
            catch (Exception ex)
            {
                m_objErrorHandler.LogInfo(ex);
                return 0;
            }
        }//GetPortfolioValueOnStartDate

        private double GetPortfolioReturnOnDate(DateTime stepDate, out int secsCount, out double portAmount)
        {
            // Rates are not used now (...RatesBacktesting), delete code later
            double secWeight, secQty, secPrice;
            double portReturn = 0;
            secsCount = 0;
            portAmount = 0;
            List<Rate> arrDR;
            List<Entities.dbo.Price> arrPrice;
            DataRow[] arrDrQty;

            try
            {

                for (int iSecs = 0; iSecs < m_objEfHandler.SecuritiesCol.Count; iSecs++)
                    if (m_objEfHandler.SecuritiesCol[iSecs].Weight > 0)
                    {
                        // Calculate Portfolio return
                        secWeight = m_objEfHandler.SecuritiesCol[iSecs].Weight;
                        arrDR = m_objEfHandler.SecuritiesCol[iSecs].RatesClass.RatesBacktesting.Where(x => x.Date == stepDate).ToList(); // should find 1 row on that day
                        if (arrDR.Count > 0)
                        {
                            portReturn += arrDR[0].RateVal.Value * secWeight;

                            // Calculate Portfolio Value
                            arrDrQty = m_dtAllVals.Select(string.Format("idSecurity = '{0}'", m_objEfHandler.SecuritiesCol[iSecs].Properties.PortSecurityId));
                            arrPrice = m_objEfHandler.SecuritiesCol[iSecs].PriceTable.Where(x => x.dDate.Date == stepDate.Date).ToList();          // should find 1 row on that day

                            if (arrDrQty.Length > 0 && arrPrice.Count > 0)
                            {
                                secQty = Convert.ToDouble(arrDrQty[0]["dQtyNew"]);    // both quantities are the same
                                secPrice = Convert.ToDouble(arrPrice[0].fClose);    //LR:close price patch
                                portAmount += secQty * secPrice / m_AdjCoeff;  // agorot to shekels
                            }

                            // Count securities
                            secsCount++;
                        }
                    }

                return portReturn;
            }
            catch (Exception ex)
            {
                m_objErrorHandler.LogInfo(ex);
                return 0;
            }
        }//GetPortfolioReturnOnDate

        private int SetSecuritiesWithWeightNumber()
        { //Counts securities with Weight > 0 in portfolio
            m_countPortSecs = 0;

            for (int i = 0; i < m_objEfHandler.SecuritiesCol.Count; i++)
            {
                if (m_objEfHandler.SecuritiesCol[i].Weight > 0)
                    m_countPortSecs++;
            }

            return m_countPortSecs;
        }//GetSecuritiesWithWeightNumber

        //private Boolean getPricesDataTablesInDaterange()
        //{ // Fills all prices datatables (for securities / benchmark)
        //    if (m_objEfHandler.SecuritiesCol.Count == 0) return false;

        //    try
        //    {
        //        m_dtReturnByDate = getPortBenchReturnDataStruct();
        //        m_dtSecsPricesInDateRange = cDbOleConnection.FillDataTable(cSqlStatements.getSecurityPricesByCollection(m_objEfHandler.SecuritiesCol, m_drDateRange, true), m_objPortfolio.OleDBConn.dbConnection);
        //        m_dtBMPricesInDateRange = cDbOleConnection.FillDataTable(cSqlStatements.getSecurityPrices(m_drDateRange, m_strBenchID), m_objPortfolio.OleDBConn.dbConnection);
        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex); return false;
        //    }
        //    return true;
        //}//getPricesDataTablesInDaterange Portfolio and Benchmark Returns in date range

        private DataTable getPortBenchReturnDataStruct()
        { // Retrieves the structure of the Datatable containing the portfolio + benchmark return
            DataTable dtFinal = new DataTable();
            dtFinal.Columns.Add(new DataColumn("StartDate", typeof(DateTime)));
            dtFinal.Columns.Add(new DataColumn("PortAmount", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("PortReturn", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("IndexAmount", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("IndexReturn", typeof(double)));
            //dtFinal.Columns.Add(new DataColumn("IndexReturnStdDev", typeof(double)));
            return dtFinal;
        }//getPortBenchReturnDataStruct

        #endregion Risk Modification

        #region Database methods

        public List<Models.App.PriceReturn> GetPriceReturnsInDateRange(List<string> secIDs, DateTime fromDT, DateTime toDT, string calcCurrency)
        {
            Dictionary<string, Tuple<object, NHibernate.Type.IType>> param = new Dictionary<string, Tuple<object, NHibernate.Type.IType>>();
            param.Add("security_id_list", new Tuple<object, NHibernate.Type.IType>(string.Join(",", "'" + secIDs + "'"), NHibernate.NHibernateUtil.String));
            param.Add("date_start", new Tuple<object, NHibernate.Type.IType>(fromDT, NHibernate.NHibernateUtil.DateTime));
            param.Add("date_end", new Tuple<object, NHibernate.Type.IType>(toDT, NHibernate.NHibernateUtil.DateTime));
            param.Add("calc_currency", new Tuple<object, NHibernate.Type.IType>(calcCurrency, NHibernate.NHibernateUtil.String));

            var priceReturns = repository.ExecuteSp<Entities.dbo.PriceReturn>("getPriceReturnsInDateRange", param); 
            return AutoMapper.Mapper.Map<List<Models.App.PriceReturn>>(priceReturns);
        }

        #endregion Database methods

        #region Data handling methods

        #region Original Portfolio Datatable handling

        public void fillOriginalPortfolioDatatable(DateTime? currentDT = null)
        { // Fills the given datatable with the proper values
            if (m_dtAllVals == null) m_dtAllVals = getOriginalPortDatatableStruct();
            else m_dtAllVals.Clear();
            try
            {
                int cnt = 0;   //, iSecPos = 0;
                double dSecValNew = 0D, dPriceNew = 0D, dQtyNew = 0D;
                double dSecValOrig = 0D, dPriceOrig = 0D;
                double profitLoss = 0D, profitLossPerc = 0D;

                double dCashNew = 0D, dCashOrig = 0D;

                if (currentDT == null)
                    m_colLastPricesCurrent = m_colLastPricesEnd;
                else
                    m_colLastPricesCurrent = getLastPricesCol((DateTime)currentDT);     // Replaced m_colLastPricesEnd to m_colLastPricesCurrent

                for (int iSecs = 0; iSecs < m_objEfHandler.SecuritiesCol.Count; iSecs++)
                    if (m_objEfHandler.SecuritiesCol[iSecs].Weight > 0D)
                        if (m_objEfHandler.SecuritiesCol[iSecs].PriceTable.Count > 0)
                        {
                            cnt += 1;
                            
                            // Calc new
                            dSecValNew = m_objEfHandler.SecuritiesCol[iSecs].Weight * m_dPortEquity;
                            dPriceNew = m_colLastPricesStart[iSecs];
                            dQtyNew = dSecValNew / dPriceNew * m_AdjCoeff; ;  // double division is multiplication
                            dQtyNew = double.IsInfinity(dQtyNew) ? 0 : dQtyNew;
                            if (cProperties.IsWithCash)
                            {
                                dSecValNew = Math.Floor(dQtyNew) * dPriceNew / m_AdjCoeff;
                                dCashNew += (dQtyNew - Math.Floor(dQtyNew)) * dPriceNew / m_AdjCoeff;
                                dSecValNew = double.IsInfinity(dSecValNew) ? 0 : dSecValNew;
                                dCashNew = double.IsInfinity(dCashNew) ? 0 : dCashNew;
                            }


                            // Calc original
                            if (m_colLastPricesStart[iSecs] == 0D)
                                dSecValOrig = 0D;
                            else
                                dSecValOrig = dSecValNew + (((m_colLastPricesCurrent[iSecs] / m_colLastPricesStart[iSecs]) - 1) * dSecValNew);
                            dPriceOrig = m_colLastPricesCurrent[iSecs];
                            //dQtyOrig = dSecValOrig / dPriceOrig * m_AdjCoeff;  // double division is multiplication

                            if (cProperties.IsWithCash)
                            {
                                // dQtyOrig always equals dQtyNew (warning: it chopes decimal digits and rounded dQtyOrig, so HAVE to use dQtyNew)
                                // Also dSecValOrig is calculated on the base of dSecValNew, which will be adjusted already,
                                // so there is no need for the next line
                                // dSecValOrig = Math.Floor(dQtyNew) * dPriceOrig / m_AdjCoeff;
                                dCashOrig += (dQtyNew - Math.Floor(dQtyNew)) * dPriceOrig / m_AdjCoeff;
                                dCashOrig = double.IsInfinity(dCashOrig) ? 0 : dCashOrig;
                            }

                            // Calculates portfolio profit
                            profitLoss = dSecValOrig - dSecValNew;
                            profitLossPerc = 100 * (dSecValOrig - dSecValNew) / dSecValNew;
                            profitLossPerc = double.IsInfinity(profitLossPerc) ? 0 : profitLossPerc;
                            //strUpDown = (profitLoss < 0) ? "down" : "up";

                            m_dtAllVals.Rows.Add(cnt, m_objEfHandler.SecuritiesCol[iSecs].Properties.PortSecurityId.Replace("-", "")
                                                    , m_objEfHandler.SecuritiesCol[iSecs].Properties.SecurityDisplay
                                                    // NEW - added 1 line by Laura for coloring sec caption by risk value
                                                    , m_objEfHandler.SecuritiesCol[iSecs].StdYield
                                                    , dSecValNew, dPriceNew, dQtyNew, dSecValOrig, dPriceOrig //, dQtyOrig
                                                    , profitLoss, profitLossPerc
                                                    , m_objEfHandler.SecuritiesCol[iSecs].Properties.Market.ID.ToString()
                                                    , m_objEfHandler.SecuritiesCol[iSecs].Properties.Sector.ID.ToString()
                                                    , m_objEfHandler.SecuritiesCol[iSecs].Properties.SecurityType.ID.ToString()
                                                    // New - added by Laura for ctlOptimalPortSecurities
                                                    , m_objEfHandler.SecuritiesCol[iSecs].Properties.Market.ItemName.ToString()
                                                    , m_objEfHandler.SecuritiesCol[iSecs].Properties.Sector.ItemName.ToString()
                                                    , m_objEfHandler.SecuritiesCol[iSecs].Properties.SecurityType.ItemName.ToString()
                                                    , m_objEfHandler.SecuritiesCol[iSecs].Weight    //!!!!!!!!!!!!!!!!!!!!!!!!!TEMPORARY FOR ALEX
                                                    , m_objEfHandler.SecuritiesCol[iSecs].Properties.SecuritySymbol
                                                    );

                        }
                if (cProperties.IsWithCash)
                {
                    // Add 1 line for CASH
                    double cashNew = 100 * (dCashOrig - dCashNew) / dCashNew;
                    cashNew = double.IsInfinity(cashNew) ? 0 : cashNew;
                    m_dtAllVals.Rows.Add(cnt + 1, "",
                                                "CASH",
                                                -1,  // Standard Deviation for coloring security
                                                     //  dSecValNew, dPriceNew, dQtyNew, dSecValOrig, dPriceOrig, //dQtyOrig
                                                dCashNew, 0, 0, dCashOrig, 0, //0,
                                                dCashOrig - dCashNew, cashNew,     //profitLoss, profitLossPerc
                                                "",
                                                "",
                                                "",
                                                "",
                                                "",
                                                "",
                                                0,       //!!!!!!!!!!!!!!!!!!!!!!!!!TEMPORARY FOR ALEX
                                                ""
                                                );
                }

            }
            catch (Exception ex)
            {
                m_objErrorHandler.LogInfo(ex);
            }
        }//fillOriginalPortfolioDatatable

        private DataTable getOriginalPortDatatableStruct()
        { // Retrieves the structure for the given datatable
            DataTable dtFinal = new DataTable();
            dtFinal.Columns.Add(new DataColumn("SeqNum", typeof(int)));
            dtFinal.Columns.Add(new DataColumn("idSecurity", typeof(string)));
            dtFinal.Columns.Add(new DataColumn("strSecName", typeof(String)));
            //LR - added 2 risk fields for new sec display control to paint sec caption by risk
            dtFinal.Columns.Add(new DataColumn("standardDev", typeof(double)));

            dtFinal.Columns.Add(new DataColumn("dValNew", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("dPriceNew", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("dQtyNew", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("dValOrig", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("dPriceOrig", typeof(double)));
            //dtFinal.Columns.Add(new DataColumn("dQtyOrig", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("ProfitLoss", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("ProfitLossPerc", typeof(double)));
            // Columns with Repository data
            dtFinal.Columns.Add(new DataColumn("marketId", typeof(string)));
            dtFinal.Columns.Add(new DataColumn("sectorId", typeof(string)));
            dtFinal.Columns.Add(new DataColumn("securityTypeId", typeof(string)));
            // Columns to be used in ctlBacktestingSecurities
            dtFinal.Columns.Add(new DataColumn("marketName", typeof(string)));
            dtFinal.Columns.Add(new DataColumn("sectorName", typeof(string)));
            dtFinal.Columns.Add(new DataColumn("securityTypeName", typeof(string)));
            dtFinal.Columns.Add(new DataColumn("dWeight", typeof(double)));   //!!!!!!!!!!!!!!!!!!!!!!!!!TEMPORARY FOR ALEX
            dtFinal.Columns.Add(new DataColumn("strSymbol", typeof(String)));
            return dtFinal;
        }//getOriginalPortDatatableStruct

        #endregion Original Portfolio Datatable handling

        #region Security Weights methods - NOT USED NOW

        //private void fillSecuritiesWeightsDT()
        //{ // NOT USED NOW 
        //  // Fills in table with security Weight, Amount and Quantity
        //    try
        //    {
        //        string idSecurity;
        //        double secAmount, secWeight, secQuantity, secPriceOnFirstDate;

        //        if (m_dtSecuritiesWeights == null) m_dtSecuritiesWeights = getDatatableSecsWeightsStructure();
        //        else m_dtSecuritiesWeights.Clear();

        //        for (int iSecs = 0; iSecs < m_objEfHandler.SecuritiesCol.Count; iSecs++)
        //            if (m_objEfHandler.SecuritiesCol[iSecs].Weight > 0)
        //            { // Participating security
        //                idSecurity = m_objEfHandler.SecuritiesCol[iSecs].Properties.PortSecurityId;
        //                secWeight = m_objEfHandler.SecuritiesCol[iSecs].Weight;
        //                secAmount = m_dPortEquity * secWeight;
        //                secQuantity = calculateSecurityQuantityinPortfolio(idSecurity, secAmount, out secPriceOnFirstDate);

        //                m_dtSecuritiesWeights.Rows.Add(idSecurity, secWeight, secAmount, secPriceOnFirstDate, secQuantity);
        //            }
        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex);
        //    }
        //} //FillSecuritiesWeightsDT

        //private double calculateSecurityQuantityinPortfolio(string idSecurity, double secAmount, out double secPriceOnFirstDate)
        //{ // NOT USED NOW 
        //    // Calculates security's quantity based on Amount
        //    secPriceOnFirstDate = -99999;
        //    double secQuantity = 0;

        //    try
        //    {
        //        DateTime firstDate = Convert.ToDateTime(m_dtSecsPricesInDateRange.Rows[0]["dDate"]);
        //        secPriceOnFirstDate = getSecPriceByDate(idSecurity, firstDate);
        //        if (secPriceOnFirstDate != 0)
        //            secQuantity = secAmount / secPriceOnFirstDate;    // Price is in agorot, so we have to divide by 100 [double division resulted in multiplication]

        //        return secQuantity;
        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex);
        //        return 0;
        //    }
        //}//calculateSecurityQuantityinPortfolio

        //private double getSecPriceByDate(string idSecurity, DateTime currDate)
        //{ // NOT USED NOW 
        //  // Extracts security price on given date
        //    double price = 0;
        //    try
        //    {
        //        // Get security Price on given date
        //        DataRow[] arrRows = m_dtSecsPricesInDateRange.Select(string.Format("dDate = #{0}# and idSecurity = '{1}'", currDate.ToString("yyyy/MM/dd"), idSecurity));
        //        if (arrRows.Length > 0)
        //            price = Convert.ToDouble(arrRows[0][m_strCloseFld]);

        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex); return 0;
        //    }
        //    return price;
        //}//getSecPriceByDate

        //private DataTable getDatatableSecsWeightsStructure()
        //{ // NOT USED NOW 
        //  // Retrieves the structure for the given datatable
        //    DataTable dtFinal = new DataTable();
        //    dtFinal.Columns.Add(new DataColumn("securityId", typeof(string)));
        //    dtFinal.Columns.Add(new DataColumn("dWeight", typeof(double)));
        //    dtFinal.Columns.Add(new DataColumn("Amount", typeof(double)));
        //    dtFinal.Columns.Add(new DataColumn("PriceOnFirstDate", typeof(double)));
        //    dtFinal.Columns.Add(new DataColumn("Quantity", typeof(double)));
        //    return dtFinal;
        //}//getDatatableSecsWeightsStructure

        #endregion Security Weights methods

        #endregion Data handling methods

        #region General methods

        private List<double> getLastPricesCol(DateTime dtEndDate)
        { // Creates a collection containing the last prices of securities
            double dPriceVal = 0D;
            Boolean isFound = false;
            List<double> lstFinal = new List<double>();
            for (int iSecs = 0; iSecs < m_objEfHandler.SecuritiesCol.Count; iSecs++)
                if (m_objEfHandler.SecuritiesCol[iSecs].PriceTable.Count > 0)
                { // Only if data exists
                    isFound = false;

                    for (int iRows = 0; iRows < m_objEfHandler.SecuritiesCol[iSecs].PriceTable.Count; iRows++)
                        if (m_objEfHandler.SecuritiesCol[iSecs].PriceTable[iRows].dDate <= dtEndDate)
                        { // Adds found price to collection
                            dPriceVal = Convert.ToDouble(m_objEfHandler.SecuritiesCol[iSecs].PriceTable[iRows].dAdjPrice);

                            lstFinal.Add(dPriceVal); isFound = true; break;
                        }

                    if (!isFound) lstFinal.Add(0D);
                } else lstFinal.Add(0D);

            return lstFinal;
        }//getLastPricesCol

        #endregion General methods

        #endregion Methods

        #region Properties

        public DataTable PortfolioData
        { get { return m_dtAllVals; } }//PortfolioData

        //public DataTable BenchmarkRates
        //{ get { return m_dtBMrates; } }//Benchmark Rates table by period

        public ISecurity CurrBenchmarkSec
        { get { return m_currBenchmarkSec; } }//Benchmark Security

        public List<BenchMarkResult> ReturnsByDate
        { get { return m_dtReturnByDate; } }//ReturnsByDate

        public cDateRange DateRange
        {
            get { return m_drDateRange; }
            set { m_drDateRange = value; }
        }//DateRange

        public double Equity
        {
            get { return m_dPortEquity; }
            set { m_dPortEquity = value; }
        }//Equity

        public double Profit
        { get { return m_dFinalProfit; } }//Profit

        public String CurrencySign
        { get { return m_CurrencySign; } }//CurrencySign

        public double AdjCoeff
        { get { return m_AdjCoeff; } }//coefficient to convert agorot to shekels

        public double BenchmarkRisk
        { get { return m_dBenchmarkRisk; } }//Benchmark risk

        public double BenchmarkReturn { get { return m_dBMarkReturn; } }

        public double CurrPortRiskVal
        { get { return m_dCurrPortRiskVal; } }//Current Portfolio risk, close to (BM risk + 15%)

        public double CurrPortReturnValue {  get { return m_dPortfolioReturn; } }
        public double CurrPortRateToRisk { get; set; }
        public double CurrPortDiversification { get; set; }
        public String BenchmarkID
        { 
            get { return m_strBenchID; }
            set { m_strBenchID = value; }
        }//BenchmarkID
        public double ClosestPortRiskVal
        { get { return m_dClosestPortRiskVal; } }//Closest to benchmark risk portfolio risk

        #endregion Properties

    }//of class
}
