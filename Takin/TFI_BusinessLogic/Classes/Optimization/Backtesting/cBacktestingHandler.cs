using Cherries.Models.App;
using Cherries.Models.dbo;
using Cherries.Models.ViewModel;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Optimization;
using Cherries.TFI.BusinessLogic.Securities;
using Ness.DataAccess.Repository;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFI.BusinessLogic.Bootstraper;
using TFI.BusinessLogic.Interfaces;

namespace TFI.BusinessLogic.Classes.Optimization.Backtesting
{
    public class cBacktestingHandler : IBacktestingHandler
    {

        #region Data members

        // Project variables
        private IPortfolioBL m_objPortfolio; // Current Portfolio class
        private ICollectionsHandler m_objColHandler; // Collection handler
        private IOptimizationResults m_objEfHandler; // Markowitz calculation handler
        private IErrorHandler m_objErrorHandler; // Error handler

        // Calculation parameters
        private double m_dPortEquity = 0D; // Portfolio's equity
        private cDateRange m_drDateRange = new cDateRange(DateTime.Today, DateTime.Today); // Date range for backtesting
        //private const int m_IntervalDays = 7; // Datapoints interval (per days)
        //private int m_countPortSecs = 0; // Number of securities for calculated portfolio

        // Data variables
        private DataTable m_dtMainSecurities; // Main Datatable of securities
        //private DataTable m_dtAllVals; // Original portfolio datatable
        //private DataTable m_dtSecuritiesWeights = null; // Table of securities quantities (weights) for the current portfolio
        //private DataTable m_dtSecsPricesInDateRange = null; // Table of securities prices in date range with days interval
        //private DataTable m_dtBMPricesInDateRange = null; // Table of benchmark prices in date range with days interval
        private List<Tuple<GeneralItem, List<BenchMarkResult>>> m_dtReturnByDate = new List<Tuple<GeneralItem, List<BenchMarkResult>>>(); // Table of Portfolio and Benchmark Returns in date range with days interval, ordered by Date
        private List<double> m_colLastPricesStart = new List<double>(); // Collection of last prices for securities (for profit calculation - Start of period)
        private List<double> m_colLastPricesEnd = new List<double>(); // Collection of last prices for securities (for profit calculation - End of period)
        //private List<double> m_colLastPricesCurrent = new List<double>(); // Collection of last prices for securities (for profit calculation - Current date)
        private ISecurities m_colBenchmarks; // Collection of selected benchmarks
        //private IBacktestingSecurities m_colBacktestingSecurities; // Collection of backtesting securities
        //private ISecurities m_colBenchmarks; // Collection of benchmarks
        private ISecurity m_currBenchmarkSec;   // Benchmark Security
        //private String m_strBenchID = "0";      // Benchmark ID
        private double m_dCurrPortRiskVal = 0D; // Current risk value
        private double m_dFinalProfit = 0D; // Portfolio's profit
        private double m_dPortfolioReturn = 0D; // Portfolio's return
        private double m_dBMarkReturn = 0D; // Benchmark's return
        private double m_dBenchmarkRisk = 0; // Risk of selected benchmark
        //DataTable m_dtBMrates;              // Benchmark rate table for drawing GRAPH

        // General variables
        private String m_Currency;      // Currency type
        private String m_CurrencySign;  // Currency sign (for display)
        private double m_AdjCoeff;      // Adjusted Coefficient for selected currency

        //private DateTime constMinDT;    // = new DateTime(1900, 01, 01); // constant date, indicating that current date is the end date of period
        //private double m_dClosestPortRiskVal;

        //LAURA GITHUB CHECKIN TEST

        #region Cloud
        private IRepository repository;// = new Repository();

        public DataTable SecuritiesTable { get => m_dtMainSecurities; set => m_dtMainSecurities = value; }
        public List<Tuple<GeneralItem, List<BenchMarkResult>>> benchMarkResult { get => m_dtReturnByDate; set => m_dtReturnByDate = value; }
        public double AdjCoeff { get => m_AdjCoeff; set => m_AdjCoeff = value; }
        public string CurrencySign { get => m_CurrencySign; set => m_CurrencySign = value; }
        public double Profit { get => m_dFinalProfit; set => m_dFinalProfit = value; }
        public double CurrPortRiskVal { get => m_dCurrPortRiskVal; set => m_dCurrPortRiskVal = value; }
        public double CurrPortReturnValue { get => m_dPortfolioReturn; set => m_dPortfolioReturn = value; }
        public double BenchmarkRisk { get => m_dBenchmarkRisk; set => m_dBenchmarkRisk = value; }
        public double DBMarkReturn { get => m_dBMarkReturn; set => m_dBMarkReturn = value; }
        public double DPortEquity { get => m_dPortEquity; set => m_dPortEquity = value; }
        #endregion Cloud

        #endregion Data members

        #region Consturctors, Initialization & Destructor

        public cBacktestingHandler ()
        { } // Constructor

        public void setBacktestingPortfolio(IPortfolioBL cPort, DateTime dtStartDate, DateTime dtEndDate, double dEquity)
        {
            m_objPortfolio = cPort;
            m_objColHandler = m_objPortfolio.ColHandler;
            m_objEfHandler = m_objPortfolio.Classes.Optimizer;
            m_objErrorHandler = m_objPortfolio.cErrorLog;

            try
            {
                //m_colBenchmarks = StaticData<cSecurity, ISecurities>.BenchMarks;
                DPortEquity = dEquity;
                m_drDateRange = new cDateRange(dtStartDate, dtEndDate); // For event purposes

                initMainVars();

            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//setBacktestingPortfolio

        private void initMainVars()
        { // Initializes main variables in class
            m_Currency = cGeneralFunctions.getCurrencyName(m_objPortfolio.Details.CalcCurrency, out m_CurrencySign, out m_AdjCoeff);
            //constMinDT = new DateTime(1900, 01, 01);
        }//initMainVars

        #endregion Consturctors, Initialization & Destructor

        #region Methods

        #region Data handling methods

        #region Benchmark Data handling

        private void calculateBenchAndPortPriceReturns(ISecurities cSecsCol)
        { // Builds a collection with BMark return value, portfolio return value for the period, used for drawing backtesting GRAPH
            try
            {
                // Variables
                double IndexReturn, IndexAmount = 0, prevIndexAmount = 0, cumulIndexReturn = 0; int secsCount = 0;
                double PortReturn, PortAmount = 0, prevPortAmount = 0, cumulPortReturn = 0; double BmQty = 1, dBenchNextReturn = 0D;
                DateTime stepDate, prevDate = DateTime.MinValue; PriceReturn m_drCurrRow;
                List<Entities.dbo.Price> arrRows;

                // Initializtion
                benchMarkResult = new List<Tuple<GeneralItem, List<BenchMarkResult>>>();

                if (m_colBenchmarks.Count == 0)
                    m_colBenchmarks.Add(getBlankBenchmark(m_objColHandler.Benchmarks[0])); // No Benchmarks //LR: WHY [1] and not [0]??

                //m_colBenchmarks.Add(getBlankBenchmark(cSecsCol[1])); // No Benchmarks //LR: WHY [1] and not [0]??


                for (int benchMarkId = 0; benchMarkId < m_colBenchmarks.Count; benchMarkId++)
                {
                    m_currBenchmarkSec = m_colBenchmarks[benchMarkId];
                    var key = new GeneralItem { ID = m_currBenchmarkSec.Properties.PortSecurityId, Name = m_currBenchmarkSec.Properties.SecurityName };
                    var list = new List<BenchMarkResult>();
                    dBenchNextReturn = 0D; cumulPortReturn = 0;
                    IndexReturn = 0D; IndexAmount = m_objPortfolio.Details.Equity; prevIndexAmount = m_objPortfolio.Details.Equity; cumulIndexReturn = 0;

                    // Benchmark Quantity
                    stepDate = Convert.ToDateTime(m_drDateRange.StartDate);
                    arrRows = m_currBenchmarkSec.PriceTable.Where(x => x.dDate.Date == stepDate.Date).ToList();
                    if (arrRows.Count > 0)
                    { // Benchmark has values in given date - Calculate Quantity
                        IndexAmount = Convert.ToDouble(arrRows[0].fClose);      //LR:close price patch
                        if (IndexAmount == 0D)
                            BmQty = (IndexAmount == 0D) ? 0D : DPortEquity / IndexAmount * AdjCoeff;
                    }

                    prevPortAmount = m_objPortfolio.Details.Equity;
                    // CALCULATE PORTFOLIO RETURN
                    // Main Loop (over price returns)
                    //for (int i = m_currBenchmarkSec.RatesClass.PriceReturns.Count - 1; i >= 0; i--)
                    for (int i = 0; i < m_currBenchmarkSec.RatesClass.PriceReturns.Count; i++)
                        if ((m_currBenchmarkSec.RatesClass.PriceReturns[i].dtDate >= m_drDateRange.StartDate) && (m_currBenchmarkSec.RatesClass.PriceReturns[i].dtDate <= m_drDateRange.EndDate))
                        { // Goes through entire collection of price returns for current Benchmark security

                            m_drCurrRow = m_currBenchmarkSec.RatesClass.PriceReturns[i];
                            stepDate = Convert.ToDateTime(m_drCurrRow.dtDate);

                            // Handle prices for current date
                            PortReturn = getPortfolioReturnOnDate(stepDate, out secsCount, out PortAmount, prevPortAmount, prevDate);
                            if ((PortReturn == 0D) || (PortReturn == -1D)) // First row
                            { PortAmount = prevPortAmount; IndexAmount = prevIndexAmount; } // = PortAmount; } Why was it here ??????????

                            cumulPortReturn = (PortAmount - DPortEquity) / DPortEquity;

                            IndexReturn = dBenchNextReturn;
                            dBenchNextReturn = Convert.ToDouble(m_drCurrRow.dReturn);
                            //IndexAmount = (double)m_currBenchmarkSec.RatesClass.RatesBacktesting[i].dPriceClose;
                            IndexAmount = prevIndexAmount + (prevIndexAmount * IndexReturn);
                            if (list.Count > 0)
                                cumulIndexReturn = (IndexAmount / (double)list[0].IndexAmount) - 1;
                            //cumulIndexReturn = ((IndexAmount / AdjCoeff) * BmQty - DPortEquity) / DPortEquity;  // agorot to shekels    //double division is multiplication

                            if (m_currBenchmarkSec.Properties.PortSecurityId == "0000")
                            { // No Benchmark Selected
                                cumulIndexReturn = 0D;
                                IndexAmount = 0D;
                            }

                            list.Add(new BenchMarkResult { StartDate = stepDate, PortAmount = PortAmount, PortReturn = cumulPortReturn, IndexAmount = IndexAmount, IndexReturn = cumulIndexReturn });
                            // list.Add(new BenchMarkResult { StartDate = stepDate, PortAmount = PortAmount, PortReturn = cumulPortReturn, IndexAmount = IndexAmount, IndexReturn = IndexReturn });


                            

                            prevPortAmount = PortAmount;
                            prevIndexAmount = IndexAmount;
                            prevDate = stepDate;
                            //}
                        }//Main for loop

                    //LR: Commenting - It looks like in WEB version we don't need it
                    //// Because weekly division starts from the end of period till the beginning, we might miss the beginning date, so adding it manualy
                    //stepDate = Convert.ToDateTime(m_drDateRange.StartDate);
                    //IndexReturn = 0;
                    //arrRows = m_currBenchmarkSec.PriceTable.Where(x => x.dDate == stepDate).ToList();


                    benchMarkResult.Add(new Tuple<GeneralItem, List<BenchMarkResult>>(key, list));



                    
                }
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            } 
        }//calculateBenchAndPortPriceReturns

        //public void fillPortAndBMarksReturnsTable()
        //{ fillPortAndBMarksReturnsTable(m_drDateRange.StartDate, m_drDateRange.EndDate); }//fillPortAndBMarksReturnsTable

        //public void fillPortAndBMarksReturnsTable(DateTime dtStart, DateTime dtEnd)
        //{ // Builds table with BMark return value, portfolio return value for the period, used for drawing backtesting GRAPH

        //    try
        //    {
        //        // Variables
        //        double IndexReturn, IndexAmount = 0, prevIndexAmount =0, cumulIndexReturn = 0; int secsCount = 0;
        //        double PortReturn, PortAmount = 0, prevPortAmount = 0, cumulPortReturn = 0; double BmQty = 1, dBenchNextReturn = 0D;
        //        DateTime stepDate, prevDate = DateTime.MinValue; PriceReturn m_drCurrRow;
        //        List<Entities.dbo.Price> arrRows;
                
        //        // Initializtion
        //        benchMarkResult = new List<Tuple<GeneralItem, List<BenchMarkResult>>>();

        //        if (m_colBenchmarks.Count == 0)
        //            m_colBenchmarks.Add(getBlankBenchmark(m_colBacktestingSecurities.Securities[1])); // No Benchmarks

        //        for (int benchMarkId = 0; benchMarkId < m_colBenchmarks.Count; benchMarkId++)
        //        {
        //            m_currBenchmarkSec = m_colBenchmarks[benchMarkId];
        //            var key = new GeneralItem { ID = m_currBenchmarkSec.Properties.PortSecurityId, Name = m_currBenchmarkSec.Properties.SecurityName };
        //            var list = new List<BenchMarkResult>();
        //            dBenchNextReturn = 0D; cumulPortReturn = 0;
        //            IndexReturn = 0D; IndexAmount = m_objPortfolio.Details.Equity; prevIndexAmount = m_objPortfolio.Details.Equity; cumulIndexReturn = 0;

        //            // Benchmark Quantity
        //            stepDate = Convert.ToDateTime(dtStart);
        //            arrRows = m_currBenchmarkSec.PriceTable.Where(x => x.dDate.Date == stepDate.Date).ToList();
        //            if (arrRows.Count > 0)
        //            { // Benchmark has values in given date - Calculate Quantity
        //                IndexAmount = Convert.ToDouble(arrRows[0].fClose);      //LR:close price patch
        //                if (IndexAmount == 0D)
        //                    BmQty = (IndexAmount == 0D) ? 0D : DPortEquity / IndexAmount * AdjCoeff;
        //            }

        //            prevPortAmount = m_objPortfolio.Details.Equity;
        //            // CALCULATE PORTFOLIO RETURN
        //            // Main Loop (over price returns)
        //            for (int i = 0; i < m_currBenchmarkSec.RatesClass.PriceReturns.Count; i++)
        //            { // Goes through entire collection of price returns for current Benchmark security

        //                m_drCurrRow = m_currBenchmarkSec.RatesClass.PriceReturns[i];
        //                stepDate = Convert.ToDateTime(m_drCurrRow.ValueDT);

        //                // Handle prices for current date
        //                PortReturn = getPortfolioReturnOnDate(stepDate, out secsCount, out PortAmount, prevPortAmount, prevDate);
        //                if ((PortReturn == 0D) || (PortReturn == -1D)) // First row
        //                    { PortAmount = prevPortAmount; IndexAmount = prevIndexAmount = PortAmount; }
        //                cumulPortReturn = (PortAmount - DPortEquity) / DPortEquity;

        //                IndexReturn = dBenchNextReturn;
        //                dBenchNextReturn = Convert.ToDouble(m_drCurrRow.dReturn);
        //                //IndexAmount = (double)m_currBenchmarkSec.RatesClass.RatesBacktesting[i].dPriceClose;
        //                IndexAmount = prevIndexAmount + (prevIndexAmount * IndexReturn);
        //                if (list.Count > 0)
        //                    cumulIndexReturn = (IndexAmount / (double)list[0].IndexAmount) - 1;
        //                //cumulIndexReturn = ((IndexAmount / AdjCoeff) * BmQty - DPortEquity) / DPortEquity;  // agorot to shekels    //double division is multiplication

        //                if (m_currBenchmarkSec.Properties.PortSecurityId == "0000")
        //                { // No Benchmark Selected
        //                    cumulIndexReturn = 0D;
        //                    IndexAmount = 0D;
        //                }

        //                list.Add(new BenchMarkResult { StartDate = stepDate, PortAmount = PortAmount, PortReturn = cumulPortReturn, IndexAmount = IndexAmount, IndexReturn = cumulIndexReturn });

        //                prevPortAmount = PortAmount;
        //                prevIndexAmount = IndexAmount;
        //                prevDate = stepDate;
        //                //}
        //            }//Main for loop

        //            // Because weekly division starts from the end of period till the beginning, we might miss the beginning date, so adding it manualy
        //            stepDate = Convert.ToDateTime(dtStart);
        //            IndexReturn = 0;
        //            arrRows = m_currBenchmarkSec.PriceTable.Where(x => x.dDate == stepDate).ToList();
        //            //if (arrRows.Count > 0)
        //            //{ // Only if there is price data found
        //            //    //IndexAmount = Convert.ToDouble(m_currBenchmarkSec.IdCurrency == "9001" ? arrRows[0].fClose : arrRows[0].fNISClose); //LR:close price patch

        //            //    PortAmount = getPortfolioValueOnStartDate(out secsCount);
        //            //    if (secsCount == m_countPortSecs)
        //            //    {
        //            //        var res = new BenchMarkResult();
        //            //        res.StartDate = stepDate;
        //            //        res.PortAmount = PortAmount;
        //            //        res.PortReturn = 0;
        //            //        res.IndexAmount = IndexAmount;
        //            //        res.IndexReturn = 0;
        //            //        list.Insert(0, res);
        //            //    }
        //            //}
        //            benchMarkResult.Add(new Tuple<GeneralItem, List<BenchMarkResult>>(key, list));
        //        }
            
        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex);
        //    }
        //}//fillPortAndBMarksReturnsTable

        private ISecurity getBlankBenchmark(ISecurity cDemoSec)
        { // Creates an instance of a benchmark (without real data)
            ISecurity cEmptyBench = new cSecurity(m_objPortfolio, "Empty", "");
            cEmptyBench.Properties.PortSecurityId = "0000";

            if (cDemoSec.PriceTable.Count > 0)
            {
                cEmptyBench.PriceTable = cDemoSec.PriceTable;

                List<PriceReturn> colTempRates = new List<PriceReturn>();

                // WHY WAS IT IN DESCENDING ORDER???
                //for (int iRows = cDemoSec.RatesClass.PriceReturns.Count - 1; iRows >= 0; iRows--)
                for (int iRows = 0; iRows < cDemoSec.RatesClass.PriceReturns.Count; iRows++)
                    colTempRates.Add(cDemoSec.RatesClass.PriceReturns[iRows]);

                cEmptyBench.RatesClass.PriceReturns = colTempRates;

                //cEmptyBench.RatesClass.RatesBacktesting = cDemoSec.RatesClass.RatesBacktesting;
            }

            return cEmptyBench;
        }//getBlankBenchmark

        //private int setSecuritiesWithWeightNumber()
        //{ //Counts securities with Weight > 0 in portfolio
        //    m_countPortSecs = 0;
        //    for (int i = 0; i < m_objEfHandler.SecuritiesCol.Count; i++)
        //        if (m_objEfHandler.SecuritiesCol[i].Weight > 0)
        //            m_countPortSecs++;

        //    return m_countPortSecs;
        //}//setSecuritiesWithWeightNumber

        //private void setBenchmarkRatesclassValues(ISecurities cSecsCol)
        //{ // Fills the each benchmark security with the proper returns in the RatesClass table
        //    for (int iSecs = 0; iSecs < cSecsCol.Count; iSecs++)
        //    {
        //        cSecsCol[iSecs].RatesTable.Clear();

        //        for (int iRecs = 0; iRecs < cSecsCol[iSecs].RatesClass.PriceReturns.Count; iRecs++)
        //        {
        //            Rate rtCurr = new Rate();
        //            rtCurr.Date = cSecsCol[iSecs].RatesClass.PriceReturns[iRecs].ValueDT;
        //            rtCurr.RateVal = cSecsCol[iSecs].RatesClass.PriceReturns[iRecs].dReturn;

        //            cSecsCol[iSecs].RatesTable.Add(rtCurr);
        //        }//rates for

        //    }//securities for


        //}//setBenchmarkRatesclassValues

        #endregion Benchmark Data handling

        #region Database methods

        //public List<PriceReturn> GetPriceReturnsInDateRangeInSecCol(IBacktestingSecurities cSecsCol, DateTime fromDT, DateTime toDT, string calcCurrency)
        //{ // Retrieves collection of pre-calculated price-returns for a given collection of securities and date-range
            
        //    // CALL PRICES FROM SP
        //    List<string> secIDs = getSecIDsString(cSecsCol);
        //    List<PriceReturn> res = new List<PriceReturn>();
        //    Dictionary<string, Tuple<object, NHibernate.Type.IType>> param = new Dictionary<string, Tuple<object, NHibernate.Type.IType>>();

        //    param.Add("security_id_list", new Tuple<object, NHibernate.Type.IType>(string.Join(",", secIDs), NHibernate.NHibernateUtil.StringClob));
        //    param.Add("date_start", new Tuple<object, NHibernate.Type.IType>(fromDT, NHibernate.NHibernateUtil.DateTime));
        //    param.Add("date_end", new Tuple<object, NHibernate.Type.IType>(toDT, NHibernate.NHibernateUtil.DateTime));
        //    param.Add("calc_currency", new Tuple<object, NHibernate.Type.IType>(calcCurrency, NHibernate.NHibernateUtil.String));
        //    repository = Resolver.Resolve<IRepository>();
        //    var priceReturns = repository.ExecuteSp<Entities.dbo.PriceReturn>("getPriceReturnsInDateRange", param);
            
            
        //    // SPLIT PRICES

        //    //  Split main Table of PriceReturns amongst Securities in Securities -> RateData -> RatesBacktesting (cSecsCol[i].RatesClass.RatesBacktesting)
        //    for (int i = 0; i < cSecsCol.BacktestingSecurities.Count; i++)
        //    {
        //        var securityQry = priceReturns.Where(x => x.LocalID == cSecsCol.BacktestingSecurities[i].Security.Properties.PortSecurityId);
        //        var ratesBackt = securityQry.Select(x => new Cherries.Models.App.PriceReturn
        //        {
        //            LocalID = x.LocalID,
        //            idCurrency = x.idCurrency,
        //            ValueDT = x.ValueDT,
        //            dReturn = x.dReturn,
        //            dAdjBasePrice = x.dAdjBasePrice,
        //            dYearlyReturn = x.dYearlyReturn,
        //            dFacAccum = x.dFacAccum,
        //            dYearlyAdjBasePrice = x.dYearlyAdjBasePrice,
        //            dPriceClose = x.dPriceClose,
        //            dFacAccumTotal = x.dFacAccumTotal,
        //            dPriceBaseWeekStart = x.dPriceBaseWeekStart


        //            //Date = x.ValueDT,
        //            //RateVal = x.dReturn

        //            // Do we need to assign value to 2 last fields also?
        //            //public DateTime Date { get; set; }
        //            //public double? RateVal { get; set; }
        //            //public string Label { get; set; }
        //            //public string Tooltip { get; set; }

        //        }).OrderByDescending(x => x.ValueDT).ToList();

        //        cSecsCol.BacktestingSecurities[i].Security.RatesClass.PriceReturns = ratesBackt;
        //    }
        //    res = AutoMapper.Mapper.Map<List<PriceReturn>>(priceReturns);
        //    return res;
        //}//GetPriceReturnsInDateRangeInSecCol

        //private List<string> getSecIDsString(IBacktestingSecurities cSecsCol)
        //{// Get list of securities IDs from given securities collection
        //    List<string> lstFinal = new List<string>();
        //    for (int i = 0; i < cSecsCol.BacktestingSecurities.Count; i++)
        //        lstFinal.Add(cSecsCol.BacktestingSecurities[i].Security.Properties.PortSecurityId);

        //    return lstFinal;
        //}//GetSecIDsString

        //private List<PriceReturn> getPricesFullDateRange(ISecurity cCurrSec, List<PriceReturn> colCurrPrices, DateTime fromDT, DateTime toDT)
        //{ // Extends the price returns collection to full date range
        //    DateTime dtStart, dtEnd;

        //    dtStart = colCurrPrices[0].ValueDT;
        //    dtEnd = colCurrPrices[colCurrPrices.Count - 1].ValueDT;

        //    int iPosStart, iPosEnd;
        //    double dReturn;
        //    if (dtStart > fromDT)
        //    {
        //        iPosStart = getDatePosition(cCurrSec.PriceTable, fromDT, true);
        //        iPosEnd = getDatePosition(cCurrSec.PriceTable, dtStart, true);

        //        dReturn = ((double)cCurrSec.PriceTable[iPosStart].fClose / (double)cCurrSec.PriceTable[iPosStart].fOpen) - 1D;
                
        //    }

        //}//getPricesFullDateRange

        private int getDatePosition(List<Entities.dbo.Price> dtPrices, DateTime destDate, bool isFromStart)
        { // Retrieves desired date from table
            if (isFromStart) // From start position
                for (int iPrices = 0; (iPrices < dtPrices.Count) || (iPrices < 7); iPrices++)
                { if (dtPrices[iPrices].dDate == destDate) return iPrices; }
            else // From end position
                for (int iPrices = dtPrices.Count - 1; (iPrices >= 0) || (iPrices > dtPrices.Count - 8); iPrices--) 
                { if (dtPrices[iPrices].dDate == destDate) return iPrices; }

            return (isFromStart) ? 0 : dtPrices.Count - 1;
        }//getDatePosition

        //private List<PriceReturn> getSecuritiesPriceReturnsInDateRange(List<ISecurity> cSecsCol, DateTime fromDT, DateTime toDT, string calcCurrency)
        //{ // Retrieves collection of pre-calculated price-returns for a given collection of securities and date-range
        //    // CALL PRICES FROM SP
        //    List<string> secIDs = getSecIDsString(cSecsCol);
        //    List<PriceReturn> res = new List<PriceReturn>();
        //    Dictionary<string, Tuple<object, NHibernate.Type.IType>> param = new Dictionary<string, Tuple<object, NHibernate.Type.IType>>();

        //    param.Add("security_id_list", new Tuple<object, NHibernate.Type.IType>(string.Join(",", secIDs), NHibernate.NHibernateUtil.StringClob));
        //    param.Add("date_start", new Tuple<object, NHibernate.Type.IType>(fromDT, NHibernate.NHibernateUtil.DateTime));
        //    param.Add("date_end", new Tuple<object, NHibernate.Type.IType>(toDT, NHibernate.NHibernateUtil.DateTime));
        //    param.Add("calc_currency", new Tuple<object, NHibernate.Type.IType>(calcCurrency, NHibernate.NHibernateUtil.String));
        //    repository = Resolver.Resolve<IRepository>();
        //    var priceReturns = repository.ExecuteSp<Entities.dbo.PriceReturn>("getPriceReturnsInDateRange", param);

        //    // SPLIT PRICES
        //    for (int i = 0; i < cSecsCol.Count; i++)
        //    { //  Split main Table of PriceReturns amongst Securities in Securities -> RateData -> RatesBacktesting (cSecsCol[i].RatesClass.RatesBacktesting)
        //        var securityQry = priceReturns.Where(x => x.LocalID == cSecsCol[i].Properties.PortSecurityId);
        //        var ratesBackt = securityQry.Select(x => new Cherries.Models.App.PriceReturn
        //        {
        //            LocalID = x.LocalID, idCurrency = x.idCurrency, ValueDT = x.ValueDT, dReturn = x.dReturn, dAdjBasePrice = x.dAdjBasePrice,
        //            dYearlyReturn = x.dYearlyReturn, dFacAccum = x.dFacAccum, dYearlyAdjBasePrice = x.dYearlyAdjBasePrice, dPriceClose = x.dPriceClose,
        //            dFacAccumTotal = x.dFacAccumTotal, dPriceBaseWeekStart = x.dPriceBaseWeekStart
        //        }).ToList();

        //        // Reverse prices
        //        //cSecsCol[i].RatesClass.RatesBacktesting = new List<PriceReturn>();
        //        //for (int iRows = ratesBackt.Count - 1; iRows >= 0; iRows--)
        //        //    cSecsCol[i].RatesClass.RatesBacktesting.Add(ratesBackt[iRows]);

        //        cSecsCol[i].RatesClass.PriceReturns = ratesBackt;
        //    }
        //    res = AutoMapper.Mapper.Map<List<PriceReturn>>(priceReturns);
        //    return res;
        //}//getSecuritiesPriceReturnsInDateRange

        //private List<string> getSecIDsString(List<ISecurity> cSecsCol)
        //{// Get list of securities IDs from given securities collection
        //    List<string> lstFinal = new List<string>();
        //    for (int i = 0; i < cSecsCol.Count; i++)
        //        lstFinal.Add(cSecsCol[i].Properties.PortSecurityId);

        //    return lstFinal;
        //}//GetSecIDsString

        #endregion Database methods

        #endregion Data handling methods

        #region Calculation Methods

        #region Portfolio Securities

        private void calculateSecuritiesDatatableNew(ISecurities cSecsCol)
        { // Calculates the Quantities and values of the portfolio securities (AFTER OPTIMIZATION)
            try
            {
                // VARIABLES
                int cnt = -1;
                double dSecValNew = 0D, dPriceNew = 0D, dQty = 0D;
                double dSecValOrig = 0D, dPriceOrig = 0D;
                double profitLoss = 0D, profitLossPerc = 0D;

                double dCashOrig = 0D;
                double dCashNew = 0D;

                m_dtMainSecurities = getOriginalPortDatatableStruct();

                //List<Entities.dbo.Price> colStart = new List<Entities.dbo.Price>();
                //List<Entities.dbo.Price> colEnd = new List<Entities.dbo.Price>();
                List<PriceReturn> colStart = new List<PriceReturn>();
                List<PriceReturn> colEnd = new List<PriceReturn>();

                //m_colLastPricesStart = getPricesOnDate(cSecsCol, m_drDateRange.StartDate);
                //m_colLastPricesEnd = getPricesOnDate(cSecsCol, m_drDateRange.EndDate);

                // MAIN LOOP
                for (int iSecs = 0; iSecs < cSecsCol.Count; iSecs++)
                    if (cSecsCol[iSecs].Weight > 0D)
                    {
                        cnt += 1;

                        // Prices
                        var nearestDiff = cSecsCol[iSecs].RatesClass.PriceReturns.Min(x => Math.Abs((x.dtDate.Date - m_drDateRange.StartDate).Ticks));
                        colStart = cSecsCol[iSecs].RatesClass.PriceReturns.Where(x => Math.Abs((x.dtDate.Date - m_drDateRange.StartDate).Ticks) == nearestDiff).ToList(); // should find 1 row on that day

                        nearestDiff = cSecsCol[iSecs].RatesClass.PriceReturns.Min(x => Math.Abs((x.dtDate.Date - m_drDateRange.EndDate).Ticks));
                        colEnd = cSecsCol[iSecs].RatesClass.PriceReturns.Where(x => Math.Abs((x.dtDate.Date - m_drDateRange.EndDate).Ticks) == nearestDiff).ToList(); // should find 1 row on that day

                        //var nearestDiff = cSecsCol[iSecs].PriceTable.Min(x => Math.Abs((x.dDate.Date - m_drDateRange.StartDate).Ticks));
                        //colStart = cSecsCol[iSecs].PriceTable.Where(x => Math.Abs((x.dDate.Date - m_drDateRange.StartDate).Ticks) == nearestDiff).ToList(); // should find 1 row on that day

                        //nearestDiff = cSecsCol[iSecs].PriceTable.Min(x => Math.Abs((x.dDate.Date - m_drDateRange.EndDate).Ticks));
                        //colEnd = cSecsCol[iSecs].PriceTable.Where(x => Math.Abs((x.dDate.Date - m_drDateRange.EndDate).Ticks) == nearestDiff).ToList(); // should find 1 row on that day
                        
                        // Original Values
                        dSecValOrig = cSecsCol[iSecs].Weight * DPortEquity;
                        //dPriceOrig = m_colLastPricesStart[cnt];
                        //dPriceOrig = (double)colStart[0].fClose;
                        dPriceOrig = (double)colStart[0].fAdjClose;
                        if ((dPriceOrig == 0D) || (dSecValOrig == 0D)) 
                        { dQty = dSecValOrig = dSecValNew = 0D; continue; }

                        // New Values
                        dQty = dSecValOrig / dPriceOrig * AdjCoeff; ;  // double division is multiplication
                        dQty = double.IsInfinity(dQty) ? 0 : dQty;
                        if (cProperties.IsWithCash)
                        { // Calculates cash
                            dSecValOrig = Math.Floor(dQty) * dPriceOrig / AdjCoeff;
                            dCashOrig += (dQty - Math.Floor(dQty)) * dPriceOrig / AdjCoeff + (dSecValOrig - Math.Floor(dSecValOrig)); // add cents/agorot of value
                            dSecValOrig = Math.Floor(dSecValOrig);

                            dSecValOrig = double.IsInfinity(dSecValOrig) ? 0 : dSecValOrig;
                            dCashOrig = double.IsInfinity(dCashOrig) ? 0 : dCashOrig;
                        }
                        cSecsCol[iSecs].Quantity = dQty;

                        //dPriceNew = m_colLastPricesEnd[cnt];
                        //dPriceNew = (double)colEnd[0].fClose;
                        dPriceNew = (double)colEnd[0].fAdjClose;
                        if (dPriceNew == 0D)
                        { dSecValNew = 0D; continue; }

                        //dSecValNew = dSecValOrig + (((dPriceNew / dPriceOrig) - 1) * dSecValOrig);
                        dSecValNew = Math.Floor(dQty) * dPriceNew / AdjCoeff;
                        dCashNew += (dQty - Math.Floor(dQty)) * dPriceNew / AdjCoeff + (dSecValNew - Math.Floor(dSecValNew)); // add cents/agorot of value
                        dSecValNew = Math.Floor(dSecValNew);

                        // Calculates portfolio profit
                        profitLoss = dSecValNew - dSecValOrig;
                        profitLossPerc = 100 * (dSecValNew - dSecValOrig) / dSecValOrig;
                        profitLossPerc = double.IsInfinity(profitLossPerc) ? 0 : profitLossPerc;

                        string strExchange;
                        switch (cSecsCol[iSecs].Properties.Market.ID)
                        {
                            case 1:
                                strExchange = "TASE"; break;
                            case 3:
                                strExchange = "NASDAQ"; break;
                            case 4:
                                strExchange = "NYSE"; break;
                            case 5:
                                strExchange = "AMEX"; break;
                            default:
                                strExchange = "NO SYMBOL"; break;
                        }

                        SecuritiesTable.Rows.Add(cnt, cSecsCol[iSecs].Properties.PortSecurityId.Replace("-", ""), cSecsCol[iSecs].Properties.SecurityName
                                                , cSecsCol[iSecs].CovarClass.StandardDeviation, dSecValNew, dPriceNew, dQty, dSecValOrig, dPriceOrig, profitLoss, profitLossPerc
                                                , cSecsCol[iSecs].Properties.Market.ID.ToString()
                                                , cSecsCol[iSecs].Properties.Sector.ID.ToString()
                                                , cSecsCol[iSecs].Properties.SecurityType.ID.ToString()
                                                //, cSecsCol[iSecs].Properties.Market.ItemName.ToString()
                                                , strExchange
                                                , cSecsCol[iSecs].Properties.Sector.ItemName.ToString()
                                                , cSecsCol[iSecs].Properties.SecurityType.ItemName.ToString()
                                                , cSecsCol[iSecs].Weight
                                                , cSecsCol[iSecs].Properties.SecuritySymbol); // Add security Datarow

                    }//Main loop
                if (cProperties.IsWithCash)
                    SecuritiesTable.Rows.Add(cnt + 1, "", "CASH", -1, dCashNew, 0, 0, dCashOrig, 0, 0, 0, "", "", "", "", "", "", 0, "");
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//calculateSecuritiesDatatableNew
        
        private DataTable getOriginalPortDatatableStruct()
        { // Retrieves the structure for the given datatable
            DataTable dtFinal = new DataTable();
            dtFinal.Columns.Add(new DataColumn("SeqNum", typeof(int)));
            dtFinal.Columns.Add(new DataColumn("idSecurity", typeof(string)));
            dtFinal.Columns.Add(new DataColumn("strSecName", typeof(String)));
            dtFinal.Columns.Add(new DataColumn("standardDev", typeof(double)));

            dtFinal.Columns.Add(new DataColumn("dValNew", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("dPriceNew", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("dQtyNew", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("dValOrig", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("dPriceOrig", typeof(double)));
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
            dtFinal.Columns.Add(new DataColumn("dWeight", typeof(double))); 
            dtFinal.Columns.Add(new DataColumn("strSymbol", typeof(String)));
            return dtFinal;
        }//getOriginalPortDatatableStruct

        private List<double> getPricesOnDate(ISecurities cSecsCol, DateTime dtEndDate)
        { // Creates a collection containing the last prices of securities
            double dPriceVal = 0D;
            Boolean isFound = false;
            List<double> lstFinal = new List<double>();
            for (int iSecs = 0; iSecs < cSecsCol.Count; iSecs++)
                if (cSecsCol[iSecs].Weight > 0D)
                    if (cSecsCol[iSecs].RatesClass.PriceReturns.Count > 0)
                    { // Only if data exists
                        isFound = false;

                        for (int iRows = 0; iRows < cSecsCol[iSecs].PriceTable.Count; iRows++)
                            if (Convert.ToDateTime(cSecsCol[iSecs].PriceTable[iRows].dDate) <= dtEndDate)
                            { // Adds found price to collection
                                dPriceVal = Convert.ToDouble(cSecsCol[iSecs].PriceTable[iRows].fClose);   //LR:close price patch

                                lstFinal.Add(dPriceVal); isFound = true; break;
                            }

                        if (!isFound) lstFinal.Add(0D);
                    } else lstFinal.Add(0D);

            return lstFinal;
        }//getPricesOnDate

        #endregion Portfolio Securities

        #region Portfolio

        private double getPortfolioReturnOnDate(DateTime stepDate, out int secsCount, out double portAmount, double prevPortAmount, DateTime prevDate)
        { // Retrieves backtesting portfolio return for a given EndDate
            // Variables
            double secWeight, secQty, secPrice, portReturn = 0D;
            secsCount = 0; portAmount = 0;
            //DataRow[] arrDrQty;
            List<PriceReturn> m_drCurrRow, m_drPrevRow = null;
            //List<Entities.dbo.Price> arrPrice;

            try
            {
                //for (int iSecs = 0; iSecs < SecuritiesTable.Rows.Count - 1; iSecs++)
                for (int iSecs = 0; iSecs < SecuritiesTable.Rows.Count; iSecs++)
                { // Goes through participating securities
                    if (prevDate == DateTime.MinValue)
                        break;


                    if (SecuritiesTable.Rows[iSecs]["strSecName"].ToString() == "CASH") 
                    { portAmount += Convert.ToDouble(SecuritiesTable.Rows[iSecs]["dValOrig"]); continue; }


                    secWeight = Convert.ToDouble(SecuritiesTable.Rows[iSecs]["dWeight"]);

                    secQty = Convert.ToDouble(SecuritiesTable.Rows[iSecs]["dQtyNew"]); // Get current security quantity

                    ISecurity cCurrSec = m_objColHandler.ActiveSecs.getSecurityByIdNormalSearch(SecuritiesTable.Rows[iSecs]["idSecurity"].ToString());
                    if (cCurrSec == null)
                        continue;

                    //m_drCurrRow = m_objEfHandler.SecuritiesCol[iSecs].RatesClass.RatesBacktesting.Where(x => x.ValueDT.Date == stepDate.Date).ToList(); // should find 1 row on that day
                    //m_drCurrRow = cCurrSec.RatesClass.PriceReturns.Where(x => x.dtDate.Date == stepDate.Date).ToList(); // should find 1 row on that day
                    var nearestDiff = cCurrSec.RatesClass.PriceReturns.Min(x => Math.Abs((x.dtDate.Date - stepDate.Date).Ticks));
                    m_drCurrRow = cCurrSec.RatesClass.PriceReturns.Where(x => Math.Abs((x.dtDate.Date - stepDate.Date).Ticks) == nearestDiff).ToList(); // should find 1 row on that day

                    if (prevDate != DateTime.MinValue)
                    {
                        nearestDiff = cCurrSec.RatesClass.PriceReturns.Min(x => Math.Abs((x.dtDate.Date - prevDate.Date).Ticks));
                        m_drPrevRow = cCurrSec.RatesClass.PriceReturns.Where(x => Math.Abs((x.dtDate.Date - prevDate.Date).Ticks) == nearestDiff).ToList(); // should find 1 row on that day
                    }

                    //*********** OR ***********

                    //if (prevDate != DateTime.MinValue)
                    //    m_drPrevRow = cCurrSec.RatesClass.PriceReturns.Where(x => x.dtDate.Date == prevDate.Date).ToList();

                    if (m_drCurrRow.Count == 0)
                        continue;

                    secPrice = (double)m_drCurrRow[0].fAdjClose; // Get current security price


                    //if (m_drPrevRow == null)
                    //    { portAmount += Math.Floor(secQty) * secPrice; continue; }


                    //!!!!!!! testing !!!!! //if (m_drPrevRow == null)
                    //!!!!!!! testing !!!!! //{ portAmount += secQty * secPrice; continue; }

                    //if (m_drPrevRow.Count != 0)
                    //    secQty = (prevPortAmount * secWeight) / (double)m_drPrevRow[0].dPriceClose;
                    //secPrice = Convert.ToDouble(SecuritiesTable.Rows[iSecs]["dPriceNew"]); // Get current security price

                    portAmount += Math.Floor(secQty) * secPrice;
                    // !!!!!!! testing !!!!! //portAmount += secQty * secPrice;

                    //m_drCurrRow = m_objEfHandler.SecuritiesCol[iSecs].RatesClass.RatesBacktesting.Where(x => x.ValueDT.Date == stepDate.Date).ToList(); // should find 1 row on that day
                    //if (m_drCurrRow.Count > 0)
                    //    portReturn += m_drCurrRow[0].dReturn.Value * secWeight;

                }
                //portReturn = (prevPortAmount != 0) ? ((portAmount / (double)prevPortAmount) / prevPortAmount) : 0D;
                portReturn = (prevPortAmount != 0) ? ((portAmount / (double)prevPortAmount) - 1) : 0D;



                //for (int iSecs = 0; iSecs < m_objEfHandler.SecuritiesCol.Count; iSecs++) 
                //    if (m_objEfHandler.SecuritiesCol[iSecs].Weight > 0)
                //    {
                //        // Calculate Portfolio return
                //        secWeight = m_objEfHandler.SecuritiesCol[iSecs].Weight;
                //        m_drCurrRow = m_objEfHandler.SecuritiesCol[iSecs].RatesClass.RatesBacktesting.Where(x => x.ValueDT.Date == stepDate.Date).ToList(); // should find 1 row on that day
                //        if (m_drCurrRow.Count > 0)
                //        { // Data found for given date
                //            portReturn += m_drCurrRow[0].dReturn.Value * secWeight;

                //            // Calculate Portfolio Value
                //            arrDrQty = SecuritiesTable.Select(string.Format("idSecurity = '{0}'", m_objEfHandler.SecuritiesCol[iSecs].Properties.PortSecurityId));
                //            arrPrice = m_objEfHandler.SecuritiesCol[iSecs].PriceTable.Where(x => x.dDate.Date == stepDate.Date).ToList();          // should find 1 row on that day

                //            if (arrDrQty.Length > 0 && arrPrice.Count > 0)
                //            {
                //                secQty = Convert.ToDouble(arrDrQty[0]["dQtyNew"]); // Get current security quantity
                //                secPrice = Convert.ToDouble(arrPrice[0].fClose); // Get current security price
                //                portAmount += secQty * secPrice; // / AdjCoeff;  // Calculate portfolio amount
                //            }
                //            secsCount++; // Count securities
                //        }
                //    }

                return portReturn;
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
                return 0;
            }
        }//getPortfolioReturnOnDate

        private double getPortfolioValueOnStartDate(out int secsCount)
        {// Calculates portfolio value on Start date
            secsCount = Convert.ToInt32(SecuritiesTable.Compute("COUNT(dWeight)", "dWeight > 0"));  //m_dtAllVals.Rows.Count;
            try
            {
                return Convert.ToDouble(SecuritiesTable.Compute("SUM(dValNew)", "")); //Value on 'Start date' of date range
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex); return 0;
            }
        }//getPortfolioValueOnStartDate

        #endregion Portfolio

        #region Benchmark Calculations

        //public void calculateBenchmarkRiskAndRefreshTables()
        //{// Calculates  Benchmark risk based on Standard deviation of Benchmark yields 
        //    try
        //    {
        //        double currRisk = setBenchmarkAndGetRiskByID(m_currBenchmarkSec.Properties.PortSecurityId);
        //        refreshTablesByRiskValue(currRisk);
        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex);
        //    }
        //}//calculateBenchmarkRiskAndRefreshTables

        //public double setBenchmarkAndGetRiskByID(string p_strBenchID)
        //{ // Sets Benchmark Security instance by a given ID
        //    m_currBenchmarkSec = m_objColHandler.Benchmarks.getSecurityById(p_strBenchID);
        //    BenchmarkRisk = m_currBenchmarkSec.CovarClass.StandardDeviation * 100;

        //    // Set risk spin edit control value
        //    double min = Convert.ToDouble(m_objEfHandler.RiskRange.Y);
        //    double max = Convert.ToDouble(m_objEfHandler.RiskRange.X);
        //    double currRisk;

        //    if (BenchmarkRisk < min)
        //        currRisk = Convert.ToDouble(min);
        //    else if (BenchmarkRisk > max)
        //        currRisk = Convert.ToDouble(max);
        //    else
        //        currRisk = Convert.ToDouble(BenchmarkRisk);

        //    currRisk = Convert.ToDouble(currRisk) / 100.0;
        //    int iRowInd = m_objEfHandler.getClosestRiskValIndex(currRisk); // Save closest to BM risk portfolio risk level
            
        //    //m_dClosestPortRiskVal = m_objEfHandler.Portfolios[iRowInd].Risk;
        //    return currRisk;
        //}//setBenchmarkAndGetRiskByID
        
        #endregion Benchmark Calculations

        #region Main calculation

        private void setVarsForBacktesting(IPortfolioBL cPort)
        { // Sets up variables that were defined in InstantiateVariablesForPortfolio()
            m_objPortfolio = cPort;
            m_objColHandler = cPort.ColHandler;
            m_objEfHandler = cPort.Classes.Optimizer;

            m_objColHandler.DisabledSecs.Clear();
            m_objColHandler.Securities.undisableSecuritiesList();
            /////// ALREADY FILTERED BY EXCHANGES
            /////m_objColHandler.ActiveSecs = m_objColHandler.Securities.getListOfActiveSecs();
        }//setVarsForBacktesting

        public BackTestingViewModel calculateNewBacktesting(IPortfolioBL cPort, DateTime dtStartDate, DateTime dtEndDate, double dEquity, List<string> becnhMarkIDs)
        { // Main backtesting calculation 
            setVarsForBacktesting(cPort);
            setDisabledSecsForBacktesting(cPort.ColHandler.ActiveSecs, m_drDateRange.EndDate); // Check backtesting requirements (Disable invalid secs)
            m_objColHandler.ActiveSecs = m_objColHandler.ActiveSecs.getListOfActiveSecs();
            ISecurities cSecsCol = m_objColHandler.ActiveSecs;

            m_colBenchmarks = new cSecurities(cPort);
            m_colBenchmarks.Securities.AddRange(StaticData<cSecurity, ISecurities>.BenchMarks.Securities.Where(x => becnhMarkIDs.Contains(x.Properties.PortSecurityId)));
            BackTestingViewModel vmBacktesting = new BackTestingViewModel();
            try
            {
                OptimalPortoliosViewModel optVm = m_objEfHandler.calculateNewEfficientFrontier(ref cSecsCol, m_colBenchmarks, new cDateRange(Convert.ToDateTime(m_drDateRange.StartDate).AddYears(-2), Convert.ToDateTime(m_drDateRange.StartDate)), false, true);
                if (m_objEfHandler.isSuccessful)
                { // If Successful - refreshes control
                    m_currBenchmarkSec = m_objColHandler.Benchmarks.Securities[0];

                    m_objColHandler.ActiveSecs = cSecsCol;

                    calculateSecuritiesDatatableNew(m_objColHandler.ActiveSecs);

                    m_objEfHandler.setSecuritiesWeightsCollection(m_objColHandler.ActiveSecs, optVm.PortNumA); 
                    calculateOptimalValuesForRiskLevel(m_objColHandler.ActiveSecs, optVm.PortNumA);

                    addBenchmarksToSecuritiesCollection(optVm);

                    reportBacktestingPortfolio(m_objColHandler.ActiveSecs, m_dtMainSecurities, optVm.Portfolios[optVm.PortNumA], cPort.Details.Equity);

                    vmBacktesting = AutoMapper.Mapper.Map<BackTestingViewModel>(this);
                    vmBacktesting.BenchmarkID = m_currBenchmarkSec.Properties.PortSecurityId;
                    vmBacktesting.BenchmarkRisk = m_currBenchmarkSec.StdYield;
                    //vmBacktesting.BenchmarkRisk /= 100;   //LR it is always 0
                    vmBacktesting.Portfolios = optVm.Portfolios;
                    vmBacktesting.PortNumA = optVm.PortNumA;
                    vmBacktesting.CurrPortRiskVal = optVm.Portfolios[optVm.PortNumA].Risk;
                    vmBacktesting.CurrPortReturnValue = optVm.Portfolios[optVm.PortNumA].Return;
                    vmBacktesting.CurrPortRateToRisk = (vmBacktesting.CurrPortRiskVal == 0D) ? 0D : (vmBacktesting.CurrPortReturnValue / (double)vmBacktesting.CurrPortRiskVal);

                    int iLastPos = 0;
                    if (vmBacktesting.benchMarkResult.Count > 0)
                        iLastPos = vmBacktesting.benchMarkResult[0].Item2.Count - 1;
                    if (iLastPos >= 0)
                    {
                        vmBacktesting.Equity = vmBacktesting.benchMarkResult[0].Item2[iLastPos].PortAmount;
                        vmBacktesting.Profit = vmBacktesting.Equity - cPort.Details.Equity;
                    }
                }//main if
                } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
            return vmBacktesting;
        }//calculateNewBacktesting

        private void addBenchmarksToSecuritiesCollection(OptimalPortoliosViewModel optVm)
        { // Add Benchmarks to collection of securities

            // Should we add BM if it is EMPTY security "0000" ???

            for (int iBenchmarks = 0; iBenchmarks < m_colBenchmarks.Count; iBenchmarks++)
            { // Insert benchmarks to collection
                OptimalPortfolioSecurity currSec = new OptimalPortfolioSecurity();
                currSec.idSecurity = m_colBenchmarks[iBenchmarks].Properties.PortSecurityId;
                currSec.Symbol = m_colBenchmarks[iBenchmarks].Properties.SecuritySymbol;
                currSec.HebName = m_colBenchmarks[iBenchmarks].Properties.HebName;
                currSec.Name = m_colBenchmarks[iBenchmarks].Properties.SecurityName;
                currSec.IdCurrency = m_colBenchmarks[iBenchmarks].IdCurrency;
                currSec.idMarket = m_colBenchmarks[iBenchmarks].Properties.Market.ID;
                currSec.idSector = m_colBenchmarks[iBenchmarks].Properties.Sector.ID;
                currSec.marketName = m_colBenchmarks[iBenchmarks].Properties.MarketName;
                currSec.StdYield = m_colBenchmarks[iBenchmarks].StdYield;
                currSec.FinalRate = m_colBenchmarks[iBenchmarks].AvgYield;
                currSec.idSecurityType = 106;
                currSec.Quantity = 0;
                currSec.Weight = 0;
                currSec.Value = 0;

                optVm.Portfolios[optVm.PortNumA].Securities.Add(currSec);
            }
        }//addBenchmarksToSecuritiesCollection

        //TODO: New Method
        public BackTestingViewModel getNewBacktestingPortfolio(IPortfolioBL cPort, DateTime dtStartDate, DateTime dtEndDate, double dEquity)
        { // Retrieves a Backtesting portfolio View Model, for a given portfolio
            setVarsForBacktesting(cPort);

            // Retrieves list of compared benchmarks
            List<string> becnhMarkIDs = new List<string>();

            //***************************************************************

            List<string> missingSecs = new List<string>();
            ISecurities newColl = new cSecurities(cPort);

            for (int iSecs = 0; iSecs < cPort.OpenedSecurities.Count; iSecs++)
                if (cPort.OpenedSecurities[iSecs].idSecurityType == 106)
                    becnhMarkIDs.Add(cPort.OpenedSecurities[iSecs].idSecurity);
                else
                {
                    var sec = cPort.ColHandler.Securities.getSecurityByIdNormalSearch(cPort.OpenedSecurities[iSecs].idSecurity);
                    if (sec != null)
                        newColl.Add(sec);
                    else
                        missingSecs.Add(cPort.OpenedSecurities[iSecs].idSecurity);
                }

            if (missingSecs.Count > 0)
                cPort.ColHandler.addMissingSecurities(newColl, missingSecs);

            cPort.ColHandler.ActiveSecs = newColl;
 
            //***************************************************************

            // Set selected Benchmarks
            m_colBenchmarks = new cSecurities(cPort);
            m_colBenchmarks.Securities.AddRange(StaticData<cSecurity, ISecurities>.BenchMarks.Securities.Where(x => becnhMarkIDs.Contains(x.Properties.PortSecurityId)));

            //m_currBenchmarkSec = m_colBenchmarks.Securities[0];

            BackTestingViewModel vmBacktesting = new BackTestingViewModel();
            try
            {
                OptimalPortoliosViewModel optVm = new OptimalPortoliosViewModel();
                optVm.isSuccessful = true;
                optVm.OrigPortPos = 0;
                optVm.PortNumA = 0;
                optVm.PortID = cPort.Details.ID;
                optVm.PortDetails = cPort.Details;
                optVm.Portfolios = new List<OptimalPortfolio>();

                // Create single optimal portfolio
                cSecurities cPortSecs = getPortfolioCollectionOfSecs(cPort, m_objColHandler.ActiveSecs);    //m_objColHandler.Securities
                OptimalPortfolio optP = new OptimalPortfolio();
                optP.Risk = cPort.Details.CurrentStDev;
                cPort.Details.Profit = cPort.Details.CurrEquity - cPort.Details.Equity;
                optP.Return = cPort.Details.Profit / (double)cPort.Details.Equity;
                optP.RateToRisk = (optP.Risk == 0D) ? 0 : optP.Return / (double)optP.Risk;
                optP.Securities = new List<OptimalPortfolioSecurity>();
                for (int iSecs = 0; iSecs < cPortSecs.Count; iSecs++)
                    optP.Securities.Add(Sec2OptimalSec(cPortSecs[iSecs]));
                //for (int iSecs = 0; iSecs < m_objColHandler.Securities.Count; iSecs++)
                //    if (m_objColHandler.Securities[iSecs].Weight > 0D)
                //    { // Goes through participating securities
                //optP.Securities.Add(Sec2OptimalSec(m_objColHandler.Securities[iSecs]));
                //}//Loop for securities
                optVm.Portfolios.Add(optP);
                
                calculateSecuritiesDatatableNew(m_objColHandler.ActiveSecs);                                //m_objColHandler.Securities
                calculateOptimalValuesForRiskLevel(m_objColHandler.ActiveSecs, optVm.PortNumA);             //m_objColHandler.Securities

                vmBacktesting = AutoMapper.Mapper.Map<BackTestingViewModel>(this);
                vmBacktesting.BenchmarkID = m_currBenchmarkSec.Properties.PortSecurityId;
                vmBacktesting.BenchmarkRisk = m_currBenchmarkSec.StdYield;
                vmBacktesting.Portfolios = optVm.Portfolios;
                vmBacktesting.PortNumA = optVm.PortNumA;
                vmBacktesting.CurrPortRiskVal = optP.Risk;
                vmBacktesting.CurrPortReturnValue = optP.Return;
                vmBacktesting.CurrPortRateToRisk = optP.RateToRisk;
                vmBacktesting.Profit = cPort.Details.Profit;
                vmBacktesting.Equity = cPort.Details.Equity;
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
            return vmBacktesting;
        }//getNewBacktestingPortfolio

        private cSecurities getPortfolioCollectionOfSecs(IPortfolioBL cPort, ISecurities cOrigCol)
        { // Sets shortened collection of securities (based on securities in currently opened portfolio)
            cSecurities cFinalCol = new cSecurities(cPort);

            for (int iSecs = 0; iSecs < cPort.OpenedSecurities.Count; iSecs++)
            { // Goes through portfolio securities
                ISecurity cCurrSec = cOrigCol.getSecurityByIdNormalSearch(cPort.OpenedSecurities[iSecs].idSecurity);    //.getSecurityById
                if (cCurrSec == null) continue;

                cCurrSec.Weight = cPort.OpenedSecurities[iSecs].portSecWeight;
                cCurrSec.Quantity = cPort.OpenedSecurities[iSecs].flQuantity;

                cCurrSec.StdYield = cPort.OpenedSecurities[iSecs].stdYield;
                cCurrSec.CovarClass.StandardDeviation = cPort.OpenedSecurities[iSecs].stdYield;

                //cCurrSec.RatesClass.FinalRate = cPort.OpenedSecurities[iSecs].avgYield;
                cCurrSec.AvgYield = cPort.OpenedSecurities[iSecs].avgYield;

                cFinalCol.Add(cCurrSec);
            }
            return cFinalCol;
        }//getPortfolioCollectionOfSecs

        private OptimalPortfolioSecurity Sec2OptimalSec(ISecurity origSec)
        { // Transfors a regular security to an Optimal security object
            OptimalPortfolioSecurity currSec = new OptimalPortfolioSecurity();
            currSec.Name = origSec.Properties.SecurityName;
            currSec.HebName = origSec.Properties.HebName;
            currSec.Symbol = origSec.Properties.SecuritySymbol;
            currSec.LastPrice = origSec.LastPrice;  //DO NOT USE IT!!! It is in $ and if portfolio is 'israelonly' then in agorot. So you never know what it is
            currSec.dtPriceStart = origSec.DateRange.StartDate;
            currSec.dtPriceEnd = origSec.DateRange.EndDate;
            currSec.IdCurrency = origSec.IdCurrency;
            currSec.idMarket = origSec.Properties.Market.ID;
            currSec.marketName = origSec.Properties.MarketName;
            currSec.idSector = origSec.Properties.Sector.ID;
            currSec.sectorName = origSec.Properties.Sector.ItemName;
            currSec.idSecurity = origSec.Properties.PortSecurityId;
            currSec.idSecurityType = origSec.Properties.SecurityType.ID;
            currSec.securityTypeName = origSec.Properties.SecurityType.ItemName;
            currSec.Weight = origSec.Weight;
            currSec.FinalRate = origSec.AvgYield;
            currSec.StdYield = origSec.StdYield;
            currSec.StandardDeviation = origSec.CovarClass.StandardDeviation;       // IS this field that is used for coloring?
            currSec.Quantity = origSec.Quantity;
            currSec.Value = origSec.ValueUSA;

            return currSec;
        }//Sec2OptimalSec

        private void setDisabledSecsForBacktesting(ISecurities cSecsCol, DateTime dtEndDate)
        { // Sets the disabled securities that don't meet the date-range requirement
            bool doDisableSec = false;
            int cnt;

            for (int iSecs = 0; iSecs < cSecsCol.Count; iSecs++)
            { // Goes through securities
                doDisableSec = false;
                if (cGeneralFunctions.isBelowNumberOfMonths(new cDateRange(cSecsCol[iSecs].DateRange.StartDate, dtEndDate), cProperties.MinMonthsData))
                    doDisableSec = true;
                cnt = cSecsCol[iSecs].RatesClass.PriceReturns.Count;
                if (cnt > 0)
                {
                    // Dissable sec if it does not have prices on all the tested period
                    if (Convert.ToDateTime(cSecsCol[iSecs].RatesClass.PriceReturns[0].dtDate) > m_drDateRange.StartDate.AddDays(1))
                        doDisableSec = true;

                    
                    if (Convert.ToDateTime(cSecsCol[iSecs].RatesClass.PriceReturns[cnt - 1].dtDate) < m_drDateRange.EndDate.AddDays(-1))
                        doDisableSec = true;
                }

                if (doDisableSec) cSecsCol[iSecs].disableCurrentSecurity();
            }
        }//setDisabledSecsForBacktesting

        #endregion Main calculation

        #endregion Calculation Methods

        #region Report

        private void reportBacktestingPortfolio(ISecurities cSecsCol, DataTable dtResults, OptimalPortfolio cPort, double dEquity)
        { // Writes a report (to csv file) of covariance matrix 

            try
            {
                StreamWriter writer = new StreamWriter("C:\\TempFiles\\backtestingReport.csv");

                writer.WriteLine("Optimal Portfolio");
                writer.WriteLine();

                writer.WriteLine("Date of optimization, " + m_drDateRange.StartDate.ToShortDateString());
                writer.WriteLine("Return, " + cGeneralFunctions.getFormatPercentFromDbl(cPort.Return, 2));
                writer.WriteLine("Risk, " + cGeneralFunctions.getFormatPercentFromDbl(cPort.Risk, 2));
                writer.WriteLine("Return to Risk, " + cGeneralFunctions.getFormatPercentFromDbl(cPort.RateToRisk, 2));
                writer.WriteLine("Equity, " + cGeneralFunctions.getFormatPercentFromDbl(dEquity, 2));
                writer.WriteLine("Date range, " + m_drDateRange.StartDate.ToShortDateString() + ", " + m_drDateRange.EndDate.ToShortDateString());
                writer.WriteLine();
                writer.WriteLine();
                writer.WriteLine("Stocks of portfolio over time");

                addPortfolioDatesSecurities(writer, m_drDateRange.StartDate.AddMonths(2), m_drDateRange.EndDate.AddMonths(-2));

                addPortfolioDatesPortfolio(writer, m_drDateRange.StartDate.AddMonths(2), m_drDateRange.EndDate.AddMonths(-2));

                //DataTable dtCovarMat = getCovarianceMatTable(cSecsCol);
                //addTableToStream(dtCovarMat, writer, true);

                //writer.WriteLine();
                //writer.WriteLine();
                //writer.WriteLine("טבלה סופית");
                //addTableToStream(dtResults, writer, true);

                //cClientStaticMethods.writeDataTableToCsv(dtCovarMat, "C:\\TempFiles\\covarMatrix.csv", true);
                writer.Flush();
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//reportCovarianceMatrix

        private void addPortfolioDatesSecurities(StreamWriter writer, DateTime dtFirst, DateTime dtSecond)
        { // Adds to the report 2 date reference points, including portfolio structure, profit, return + securities profit, return and prices
            try
            {
                List<PriceReturn> m_drCurrRow;
                String strTitle = "Symbol, Weight, Quantity, Price (" + m_drDateRange.StartDate.Date + "), Value, Price (" + dtFirst.Date + "), Value, Price (" + dtSecond.Date + "), Value, Price(" +
                    m_drDateRange.EndDate.Date + "), Value";
                writer.WriteLine(strTitle);
                
                for (int iSecs = 0; iSecs < SecuritiesTable.Rows.Count; iSecs++)
                { // Goes through participating securities
                    ISecurity cCurrSec = m_objColHandler.ActiveSecs.getSecurityByIdNormalSearch(SecuritiesTable.Rows[iSecs]["idSecurity"].ToString());
                    if (cCurrSec == null)
                        continue;

                    String strLine = cCurrSec.Properties.SecuritySymbol + ", " + cGeneralFunctions.getFormatPercentFromDbl(cCurrSec.Weight, 2) + ", " + cCurrSec.Quantity + ", ";

                    // Start Date
                    var nearestDiff = cCurrSec.RatesClass.PriceReturns.Min(x => Math.Abs((x.dtDate.Date - m_drDateRange.StartDate.Date).Ticks));
                    m_drCurrRow = cCurrSec.RatesClass.PriceReturns.Where(x => Math.Abs((x.dtDate.Date - m_drDateRange.StartDate.Date).Ticks) == nearestDiff).ToList(); // should find 1 row on that day
                    if (m_drCurrRow.Count == 0)
                        continue;

                    strLine += m_drCurrRow[0].fAdjClose + ", " + (cCurrSec.Quantity * m_drCurrRow[0].fAdjClose) + ", ";

                    // Mid Date 1
                    nearestDiff = cCurrSec.RatesClass.PriceReturns.Min(x => Math.Abs((x.dtDate.Date - dtFirst.Date).Ticks));
                    m_drCurrRow = cCurrSec.RatesClass.PriceReturns.Where(x => Math.Abs((x.dtDate.Date - dtFirst.Date).Ticks) == nearestDiff).ToList(); // should find 1 row on that day
                    if (m_drCurrRow.Count == 0)
                        continue;

                    strLine += m_drCurrRow[0].fAdjClose + ", " + (cCurrSec.Quantity * m_drCurrRow[0].fAdjClose) + ", ";

                    // Mid Date 2
                    nearestDiff = cCurrSec.RatesClass.PriceReturns.Min(x => Math.Abs((x.dtDate.Date - dtSecond.Date).Ticks));
                    m_drCurrRow = cCurrSec.RatesClass.PriceReturns.Where(x => Math.Abs((x.dtDate.Date - dtSecond.Date).Ticks) == nearestDiff).ToList(); // should find 1 row on that day
                    if (m_drCurrRow.Count == 0)
                        continue;

                    strLine += m_drCurrRow[0].fAdjClose + ", " + (cCurrSec.Quantity * m_drCurrRow[0].fAdjClose) + ", ";

                    // End Date
                    nearestDiff = cCurrSec.RatesClass.PriceReturns.Min(x => Math.Abs((x.dtDate.Date - m_drDateRange.EndDate.Date).Ticks));
                    m_drCurrRow = cCurrSec.RatesClass.PriceReturns.Where(x => Math.Abs((x.dtDate.Date - m_drDateRange.EndDate.Date).Ticks) == nearestDiff).ToList(); // should find 1 row on that day
                    if (m_drCurrRow.Count == 0)
                        continue;

                    strLine += m_drCurrRow[0].fAdjClose + ", " + (cCurrSec.Quantity * m_drCurrRow[0].fAdjClose);

                    writer.WriteLine(strLine);
                }//main for
                
            } catch(Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//addPortfolioDatesCalculation

        private void addPortfolioDatesPortfolio(StreamWriter writer, DateTime dtFirst, DateTime dtSecond)
        { // Adds to the report 2 date reference points, including portfolio structure, profit, return + securities profit, return and prices
            try
            {
                List<BenchMarkResult> m_drCurrRow;
                String strLine = "Portfolio, 100%, , , ";

                // Start Date
                strLine += benchMarkResult[0].Item2[0].IndexAmount + ", , ";

                // Mid Date 1
                var nearestDiff = benchMarkResult[0].Item2.Min(x => Math.Abs((x.StartDate.Date - dtFirst.Date).Ticks));
                m_drCurrRow = benchMarkResult[0].Item2.Where(x => Math.Abs((x.StartDate.Date - dtFirst.Date).Ticks) == nearestDiff).ToList(); // should find 1 row on that day
                if (m_drCurrRow.Count == 0)
                    return;

                strLine += m_drCurrRow[0].PortAmount + ", , ";

                // Mid Date 1
                nearestDiff = benchMarkResult[0].Item2.Min(x => Math.Abs((x.StartDate.Date - dtSecond.Date).Ticks));
                m_drCurrRow = benchMarkResult[0].Item2.Where(x => Math.Abs((x.StartDate.Date - dtSecond.Date).Ticks) == nearestDiff).ToList(); // should find 1 row on that day
                if (m_drCurrRow.Count == 0)
                    return;

                strLine += m_drCurrRow[0].PortAmount + ", , ";
                
                // End Date
                strLine += benchMarkResult[0].Item2[benchMarkResult[0].Item2.Count - 1].PortAmount;


                writer.WriteLine(strLine);
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//addPortfolioDatesPortfolio


        #endregion Report

        #region Risk Modification

        private void calculateOptimalValuesForRiskLevel(ISecurities cSecsCol, int iPos)
        { // Calculates all necessary steps for a given risk level
            try
            {
                if (m_objEfHandler.Portfolios.Count > 0)
                { // If there are optimal portfolios
                    CurrPortRiskVal = m_objEfHandler.Portfolios[iPos].Risk; // Set new risk value
                    CurrPortReturnValue = m_objEfHandler.Portfolios[iPos].Return;
                }
                m_objEfHandler.PortNumA = iPos;
                m_objEfHandler.setCollectionsDataForRiskLevel(iPos);

                calculateBenchAndPortPriceReturns(cSecsCol); // Fill in IndexReturn and PortReturn for GRAPH points
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//calculateOptimalValuesForRiskLevel

        #endregion Risk Modification

        #endregion Methods

        #region Properties

        public ISecurities Benchmarks
        { get { return m_colBenchmarks; } }//Benchmarks

        #endregion Properties

    }//of class
}
