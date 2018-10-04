using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;

// Used namespaces
using Cherries.TFI.BusinessLogic.Collections;
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.Portfolio;
using Cherries.TFI.BusinessLogic.Constraints;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.DataManagement.StaticMethods;
using Cherries.TFI.BusinessLogic.GMath.StaticMethods;
using Cherries.TFI.BusinessLogic.GMath;
using Cherries.TFI.BusinessLogic.General;
using Cherries.Models.ViewModel;
using TFI.BusinessLogic.Enums;
using TFI.BusinessLogic.Interfaces;
using TFI.Entities.dbo;

namespace Cherries.TFI.BusinessLogic.Optimization
{
    public class cOptimizationResults : IOptimizationResults
    {

        #region Data Members

        // Main variables
        private IPortfolioBL m_objPortfolio; // Portfolio class
        private IErrorHandler m_objErrorHandler; // Error handler
        private ICollectionsHandler m_objColHandler; // Collections handler
        private ICovarCorrelHandler m_objCovarCalcs; // Covariance calculations
        private cConstHandler m_objConstraints; // Constraints handler
        //private ISecurities m_colSecurities; // Collection of securities for calculation

        // Data variables
        private DataTable m_dtSecsData = null; // Datatable with securities data
        private List<cOptimalPort> m_colOptimalPorts = new List<cOptimalPort>(); // Optimal portfolios collection
        private double[,] m_dWeightsMatrix; // Main weights matrix
        private double[,] m_arrCovarData; // Covariance matrix
        private double[] m_arrRatesVector; // Rates vector
        private PointF m_pntRangeOfRisks = new PointF(); // Range of risk in current EF space
        private String m_strEngineErrors; // Math engine error message (if detected)
        private Boolean m_isSciError = false; // Checks whether we need to recalculate the EF (if Error is with our data and not Scilab error)

        // General variables
        private Boolean m_isSuccessfulCalc = false; // Whether the last calculation has been sucessful or not
        private int m_iPortPos; // Current portfolio position
        private int m_iPortNumOrig; // Original portfolio position
        private int m_iTangencyPortInd; // Index of tangency portfolio (in array)

        // General variables
        private String m_Currency;      // Currency type
        private String m_CurrencySign;  // Currency sign (for display)
        private double m_AdjCoeff;

        #endregion Data Members

        #region Constructors, Initialization & Destructor

        public cOptimizationResults(IPortfolioBL cPort)
        {
            m_objPortfolio = cPort;
            m_objColHandler = m_objPortfolio.ColHandler;
            //m_colSecurities = m_objPortfolio.ColHandler.ActiveSecs;
            m_objCovarCalcs = m_objPortfolio.Classes.CovarCorrel;
            m_objErrorHandler = m_objPortfolio.cErrorLog;
            m_objConstraints = m_objPortfolio.Classes.ConstHandler;

            initMainVars();

        }//constructor

        private void initMainVars()
        { // Initializes main variables in class
            m_Currency = cGeneralFunctions.getCurrencyName(m_objPortfolio.Details.CalcCurrency, out m_CurrencySign, out m_AdjCoeff);

        }//initMainVars

        #endregion Constructors, Initialization & Destructor

        #region Methods

        #region Calculation methods

        public OptimalPortoliosViewModel calculateNewEfficientFrontier(ref ISecurities cSecsCol, ISecurities cBenchmarksCol, cDateRange drCalcRange, Boolean isClearDisabled, Boolean isBacktest)
        { // Calculates the Efficient Frontier including the rates vector and covariance matrix
            /// Parameters: cSecsCol - collection of securities for optimization
            /// drCalcRange - date range for calculation
            OptimalPortoliosViewModel vmOptimal = new OptimalPortoliosViewModel();
            try
            {
                //m_colSecurities = cSecsCol;

                if (cSecsCol.Count > 0)
                { // Data exists for calculations
                    if (isClearDisabled) m_objColHandler.DisabledSecs.Clear();

                    // Calculate covariance
                    m_objPortfolio.Classes.CovarCorrel.calcRiskAndCovariance(drCalcRange, ref cBenchmarksCol, false, isBacktest);
                    m_arrCovarData = m_objPortfolio.Classes.CovarCorrel.calcRiskAndCovariance(drCalcRange, ref cSecsCol, true, isBacktest);
                    //m_colSecurities = cSecsCol;

                    // Calculate optimal portfolios
                    runEfficientFrontierModule(cSecsCol);
                    setStartingRiskLevel();
                    setSecuritiesQuantities(cSecsCol);
                    setSecuritiesDatatable(cSecsCol);   // In case of Backtesting there is no need to call this function


                    //m_objCovarCalcs.reportCovarianceMatrix(cSecsCol, m_dtSecsData, Portfolios[PortNumA], m_objPortfolio.Details.Equity, isBacktest);
                    //m_objCovarCalcs.reportCovarianceMatrix(cSecsCol, m_dtSecsData, Portfolios[0], m_objPortfolio.Details.Equity, isBacktest);
                }
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }

            setCalcSuccess();

            if (!m_isSuccessfulCalc)
                vmOptimal.Messages.Add(new Models.App.Message { Text = "Failed to calculate due to: " + m_strEngineErrors, LogLevel = Models.App.LogLevel.Fatal });
            else {
                vmOptimal = AutoMapper.Mapper.Map<OptimalPortoliosViewModel>(m_objPortfolio.Classes.Optimizer);
                vmOptimal.PortID = m_objPortfolio.Details.ID;
                vmOptimal.PortDetails = m_objPortfolio.Details;

                for (int iPorts = 0; iPorts < vmOptimal.Portfolios.Count; iPorts++)
                {
                    for (int iSecs = 0; iSecs < vmOptimal.Portfolios[iPorts].Securities.Count; iSecs++)
                    { // Securities displayed on page
                        vmOptimal.Portfolios[iPorts].Securities[iSecs].StdYield *= Math.Sqrt(52);
                        vmOptimal.Portfolios[iPorts].Securities[iSecs].FinalRate *= 52;   //vmOptimal.PortNumA
                        vmOptimal.Portfolios[iPorts].Securities[iSecs].StandardDeviation *= 1;
                    }
                }

            }
            return vmOptimal;
        }//calculateNewEfficientFrontier

        private void runEfficientFrontierModule(ISecurities cSecsCol)
        { // Runs the efficient frontier calculation
            try
            {
                // Initialize variables
                double[,] weightsMat;
                cOptimalStaticCalcs calcs = new cOptimalStaticCalcs();
                // Constraints
                m_objConstraints.refreshConstList();
                m_objConstraints.setConstraintValues(cSecsCol);

                // Parameters
                m_arrRatesVector = getRatesArrForScilab(cSecsCol); // Create rates array
                if ((m_arrRatesVector.Length == 0) || (m_arrCovarData == null)) { m_strEngineErrors = "Missing data"; m_objErrorHandler.LogInfo(m_strEngineErrors); return; }

                // Calculation
                List<double> colRisks = new List<double>(), colRates = new List<double>();
                m_strEngineErrors = calcs.runOptimizationProcess(out colRisks, out colRates, out weightsMat, m_arrCovarData, m_arrRatesVector,
                    m_objConstraints.DoubleUpperBound, m_objConstraints.DoubleLowerBound, m_objConstraints.DoubleEqualitySecs, m_objConstraints.DoubleEqualityValues, m_objConstraints.IntEqualityNum,
                    m_objConstraints.Double_NonEqualitySecs, m_objConstraints.Double_NonEqualityValues, 0, cProperties.OptimalPorts);

                if ((m_strEngineErrors != "") || (colRisks.Count == 0) || (colRates.Count == 0)) { m_isSuccessfulCalc = false; return; } // Error in calculation

                // Set final variables
                m_dWeightsMatrix = weightsMat;
                setOptimalPortsCollection(cSecsCol, colRisks, colRates);
                setMainWeightsMatrix(weightsMat);

                if (m_colOptimalPorts.Count == 0) { m_isSuccessfulCalc = false; m_strEngineErrors = "Failed to calculate EF"; return; }
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//runEfficientFrontierModule

        private void setOptimalPortsCollection(ISecurities cSecsCol, List<double> colRisks, List<double> colRates)
        { // Finalizes the optimal portfolios values (based on results from math engine)
            Boolean isFindTP = true;

            // ANNUALIZE & NORMALIZE
            double dRiskVal = 0D, dReturn = 0D;
            double dMaxRatio = 0D;
            m_colOptimalPorts.Clear();
            for (int iPorts = 0; iPorts < colRisks.Count; iPorts++)
            { // Goes through optimal portfolios
                dReturn = colRates[iPorts] * 52; // RETURN

                dRiskVal = colRisks[iPorts] * Math.Sqrt(52);
                dRiskVal = Math.Abs(cClientStaticMethods.NormalRisk(dRiskVal)); // RISK

                if (isFindTP)
                { // Whether we still search for the TP portfolio
                    if ((dReturn / dRiskVal) > dMaxRatio) dMaxRatio = (dReturn / dRiskVal);
                    else { m_iTangencyPortInd = iPorts - 1; isFindTP = false; }
                }

                m_colOptimalPorts.Add(new cOptimalPort(m_dWeightsMatrix, iPorts, dRiskVal, dReturn, cSecsCol, m_objErrorHandler, 
                    m_objPortfolio.Details.Equity, m_AdjCoeff, m_objPortfolio.Details.CalcCurrency));
            }
        }//setOptimalPortsCollection

        private double[] getRatesArrForScilab(ISecurities cSecsCol)
        { // Returns series of rates from the Scilab working matrix
            double[] ratesArr = new double[cSecsCol.Count];
            for (int iSecs = 0; iSecs < cSecsCol.Count; iSecs++)
                ratesArr[iSecs] = double.IsNaN(cSecsCol[iSecs].RatesClass.WeeklyReturn) ? 0 : cSecsCol[iSecs].RatesClass.WeeklyReturn;
            return ratesArr;
        }//getRatesArrFromScilab

        private void setStartingRiskLevel()
        { // Sets the user's prefered risk level or prefered number of securities as the starting point
            int iRiskPos = 0;

            if ((m_objPortfolio.Details.CalcType == enumEfCalculationType.BestTP) || (m_objPortfolio.Details.PreferedRisk.Name == "None"))
                iRiskPos = getPortfolioBySecsNum(10);
            else iRiskPos = getPreferedRiskValuePos();

            iRiskPos = (iRiskPos < 0) ? 0 : iRiskPos; // In case the position is -1

            this.OrigPortPos = iRiskPos;
            this.PortNumA = iRiskPos;
      
        }//setStartingRiskLevel

        private int getPreferedRiskValuePos()
        { // Retrieves the portfolio corresponding best to the user's prefered risk
            double dPortRisk = (m_objPortfolio.Details.CurrentStDev == -1D) ? m_objPortfolio.Details.PreferedRisk.UpperBound : m_objPortfolio.Details.CurrentStDev;

            //double dDiff = 0D;
            for (int iRisks = 1; iRisks < Portfolios.Count; iRisks++)
                if (Portfolios[iRisks].Risk >= dPortRisk)
                { // Compares risks
                    if (iRisks == 1) return 1;
                    else return iRisks - 1;
                }

            return Portfolios.Count - 1; //if not found - Highest Risk
        }//setPreferedRiskLevel

        private int getPortfolioBySecsNum(int iNumSecs)
        { // Retrieves the portfolio corresponding best to the user's prefered risk
            if (m_iTangencyPortInd >= 0)
                if (Portfolios[m_iTangencyPortInd].Securities.Count < 15)
                    return m_iTangencyPortInd; // Retrieves tangency portfolio if it is suitable

            for (int iPorts = 1; iPorts < Portfolios.Count; iPorts++)
                if (Portfolios[iPorts].Securities.Count <= iNumSecs)
                    return iPorts;

            return Portfolios.Count - 1; //if not found - Highest Risk
        }//getPortfolioBySecsNum

        private void setCalcSuccess()
        { m_isSuccessfulCalc = (m_strEngineErrors == ""); }//setCalcSuccess

        private void setSecuritiesQuantities(ISecurities cSecsCol)
        { // Calculates the quantity of the securities and saves to variable
            double price;
            try
            {
                // Set securities weights
                for (int iSecs = 0; iSecs < cSecsCol.Count; iSecs++)
                    if (iSecs < m_dWeightsMatrix.GetLength(1))
                        cSecsCol[iSecs].Weight = m_dWeightsMatrix[m_iPortPos, iSecs];
                //for (int iSecs = 0; iSecs < m_objPortfolio.Classes.Optimizer.SecuritiesCol.Count; iSecs++)
                //    if (iSecs < m_dWeightsMatrix.GetLength(1))
                //        m_objPortfolio.Classes.Optimizer.SecuritiesCol[iSecs].Weight = m_dWeightsMatrix[m_iPortPos, iSecs];

                //ISecurities cSecsCol = m_objPortfolio.Classes.Optimizer.SecuritiesCol;
                List<double> lstPrices = getLastPricesCol(cSecsCol, DateTime.Today.AddDays(-1));
                for (int iSecs = 0; iSecs < cSecsCol.Count; iSecs++)
                    if (cSecsCol[iSecs].PriceTable.Count > 0)
                    {
                        price = (double)lstPrices[iSecs];
                        cSecsCol[iSecs].LastPrice = price;

                        if (m_objPortfolio.Details.CalcCurrency == "9999")    //&& cSecsCol[iSecs].IdCurrency == "9999")
                            price = price / 100;

                        cSecsCol[iSecs].Quantity = (m_objPortfolio.Details.Equity * cSecsCol[iSecs].Weight) / price;  // (double)lstPrices[iSecs];
                    }
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//setSecuritiesQuantities

        private List<double> getLastPricesCol(ISecurities cSecsCol, DateTime dtEndDate)
        { // Creates a collection containing the last prices of securities
            double dPriceVal = 0D;
            //Boolean isFound = false;
            List<Price> drCurrRow;

            List<double> lstFinal = new List<double>();
            //ISecurities cSecsCol = m_objPortfolio.Classes.Optimizer.SecuritiesCol;

            for (int iSecs = 0; iSecs < cSecsCol.Count; iSecs++)
                if (cSecsCol[iSecs].PriceTable.Count > 0)
                { // Only if data exists
                    //isFound = false;

                    var nearestDiff = cSecsCol[iSecs].PriceTable.Min(x => Math.Abs((x.dDate.Date - DateTime.Today.AddDays(-1)).Ticks));
                    drCurrRow = cSecsCol[iSecs].PriceTable.Where(x => Math.Abs((x.dDate.Date - DateTime.Today.AddDays(-1)).Ticks) == nearestDiff).ToList(); // should find 1 row on that day

                    if (drCurrRow.Count == 0) // Not found
                    { lstFinal.Add(0D); continue; }

                    dPriceVal = (m_objPortfolio.Details.CalcCurrency == "9999") ? ((double)drCurrRow[0].fNISClose) : ((double)drCurrRow[0].fClose);
                    lstFinal.Add(dPriceVal);


                    //for (int iRows = 0; iRows < cSecsCol[iSecs].PriceTable.Count; iRows++)
                    //    if (Convert.ToDateTime(cSecsCol[iSecs].PriceTable[iRows].dDate) <= dtEndDate)
                    //    { // Adds found price to collection



                    //        // WE HAVE TO TAKE FOUND PRICE not the last price, which is actualy first price because table is in descending order


                    //        //dPriceVal = (m_objPortfolio.Details.CalcCurrency == "9999") ? Convert.ToDouble(cSecsCol[iSecs].PriceTable[cSecsCol[iSecs].PriceTable.Count - 1].fNISClose) : Convert.ToDouble(cSecsCol[iSecs].PriceTable[cSecsCol[iSecs].PriceTable.Count - 1].fClose);
                    //        dPriceVal = (m_objPortfolio.Details.CalcCurrency == "9999") ? Convert.ToDouble(cSecsCol[iSecs].PriceTable[iRows].fNISClose) : Convert.ToDouble(cSecsCol[iSecs].PriceTable[iRows].fClose);
                    //        lstFinal.Add(dPriceVal); isFound = true; break;
                    //    }

                    //if (!isFound) lstFinal.Add(0D);
                }
                else lstFinal.Add(0D);
            return lstFinal;
        }//fillPreviousLastPrices

        #endregion Calculation methods

        #region DataTable Methods

        public void setSecuritiesDatatable(ISecurities cSecsCol)
        { // Sets the data to be display on the gridgrpSecsDoughnut
            try
            {
                if (m_dtSecsData != null) m_dtSecsData.Clear();
                else m_dtSecsData = getSecsDataStruct();

                fillSecuritiesTableData(cSecsCol);

            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//setSecuritiesDatatable

        // Modified to use outside collection
        private void fillSecuritiesTableData(ISecurities cSecsCol)
        { // Fills the Datatable with info of participating securities
            try
            {
                // setting date for calculations, if from backtesting take 'From Date'  - !!!NOT USED IN BACKTESTING!!!
                DateTime calcDate = DateTime.Today.AddDays(-1);

                int cnt = 0;
                double dSecValue = 0D, dPrice = 0D, dQuantity = 0D, dCash = 0D;
                for (int iSecs = 0; iSecs < m_dWeightsMatrix.GetLength(1); iSecs++)
                    if (m_dWeightsMatrix[m_iPortPos, iSecs] > 0D)
                    {
                        cSecsCol[iSecs].Weight = m_dWeightsMatrix[m_iPortPos, iSecs];
                        cnt += 1;
                        

                        // Calculate Security Value, Price and Quantity
                        dSecValue = cSecsCol[iSecs].Weight * m_objPortfolio.Details.CurrEquity;  //m_objPortfolio.Details.Equity;
                        dPrice = getSecurityPrice(cSecsCol[iSecs], calcDate);
                        dQuantity =  dSecValue / dPrice * m_AdjCoeff;  // double division is multiplication
                        cSecsCol[iSecs].Quantity = dQuantity;

                        if (cProperties.IsWithCash)
                        {
                            dSecValue = Math.Floor(dQuantity) * dPrice / m_AdjCoeff;
                            dCash += (dQuantity - Math.Floor(dQuantity)) * dPrice / m_AdjCoeff;
                            // add cents/agorot of security value to cash
                            dCash += dSecValue - Math.Floor(dSecValue);
                            dSecValue = Math.Floor(dSecValue);  //whole value, without cents/agorot
                        }
                        if (dQuantity > 0D)
                            m_dtSecsData.Rows.Add(cnt, cSecsCol[iSecs].Properties.PortSecurityId, cSecsCol[iSecs].Properties.SecuritySymbol, cSecsCol[iSecs].Properties.SecurityDisplay,
                                                    cSecsCol[iSecs].CovarClass.StandardDeviation, cSecsCol[iSecs].RatesClass.FinalRate, cSecsCol[iSecs].Weight, dSecValue, dPrice, dQuantity,
                                                    cSecsCol[iSecs].Properties.Market.ID.ToString(), cSecsCol[iSecs].Properties.Sector.ID.ToString(), cSecsCol[iSecs].Properties.SecurityType.ID.ToString(),
                                                    cSecsCol[iSecs].Properties.Market.ItemName.ToString(), cSecsCol[iSecs].Properties.Sector.ItemName.ToString(),
                                                    cSecsCol[iSecs].Properties.SecurityType.ItemName.ToString());


                    }
                if (cProperties.IsWithCash)
                    // Add 1 line for CASH
                    m_dtSecsData.Rows.Add(cnt + 1,  "", "", "CASH", -1, 0, 0, dCash, 0, 0, "", "", "", "", "", "");
                m_objPortfolio.Details.Cash = dCash;

            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//fillSecuritiesTableData

        private DataTable getSecsDataStruct()
        { // Retrieves the structure of the datatable on securities
            DataTable dtFinal = new DataTable();
            dtFinal.Columns.Add(new DataColumn("SeqNum", typeof(int)));
            dtFinal.Columns.Add(new DataColumn("idSecurity", typeof(string)));
            dtFinal.Columns.Add(new DataColumn("strSymbol", typeof(String)));
            dtFinal.Columns.Add(new DataColumn("strName", typeof(String)));
            dtFinal.Columns.Add(new DataColumn("dStDev", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("dEr", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("dWeight", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("dVal", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("dRate", typeof(double)));
            dtFinal.Columns.Add(new DataColumn("dQuantity", typeof(double)));
            // Columns with Repository data
            dtFinal.Columns.Add(new DataColumn("marketId", typeof(string)));
            dtFinal.Columns.Add(new DataColumn("sectorId", typeof(string)));
            dtFinal.Columns.Add(new DataColumn("securityTypeId", typeof(string)));
            // Columns to be used in ctlOptimalPortSecurities
            dtFinal.Columns.Add(new DataColumn("marketName", typeof(string)));
            dtFinal.Columns.Add(new DataColumn("sectorName", typeof(string)));
            dtFinal.Columns.Add(new DataColumn("securityTypeName", typeof(string)));

            return dtFinal;
        }//getSecsDataStruct

        private double getSecurityPrice(ISecurity CurrSec, DateTime dtCurr)
        { // Retrieves the price of the security for the specified date
            //Boolean isFound = false;
            //double dPriceVal = 0D;
            List<Price> drCurrRow;

            var nearestDiff = CurrSec.PriceTable.Min(x => Math.Abs((x.dDate.Date - dtCurr.Date).Ticks));
            drCurrRow = CurrSec.PriceTable.Where(x => Math.Abs((x.dDate.Date - dtCurr.Date).Ticks) == nearestDiff).ToList(); // should find 1 row on that day

            if (drCurrRow.Count == 0) // Not found
                return (m_objPortfolio.Details.CalcCurrency == "9999") ? Convert.ToDouble(CurrSec.PriceTable[CurrSec.PriceTable.Count - 1].fNISClose) : Convert.ToDouble(CurrSec.PriceTable[CurrSec.PriceTable.Count - 1].fClose);

            return (m_objPortfolio.Details.CalcCurrency == "9999") ? Convert.ToDouble(drCurrRow[0].fNISClose) : Convert.ToDouble(drCurrRow[0].fClose); //Convert.ToDouble(CurrSec.PriceTable[iRows].dAdjPrice);


            //if (CurrSec.PriceTable.Count > 0)
            //{ // Only if data exists
            //    for (int iRows = 0; iRows < CurrSec.PriceTable.Count; iRows++)
            //        if (Convert.ToDateTime(CurrSec.PriceTable[iRows].dDate) <= dtCurr)
            //        { // Adds found price to collection
            //            isFound = true;
            //            dPriceVal = (m_objPortfolio.Details.CalcCurrency == "9999") ? Convert.ToDouble(CurrSec.PriceTable[iRows].fNISClose) : Convert.ToDouble(CurrSec.PriceTable[iRows].fClose); //Convert.ToDouble(CurrSec.PriceTable[iRows].dAdjPrice);
            //            return dPriceVal;
            //        }
            //    if (!isFound) return (m_objPortfolio.Details.CalcCurrency == "9999") ? Convert.ToDouble(CurrSec.PriceTable[CurrSec.PriceTable.Count - 1].fNISClose) : Convert.ToDouble(CurrSec.PriceTable[CurrSec.PriceTable.Count - 1].fClose);
            //}
            //return dPriceVal;
        }//getSecurityPrice

        #endregion DataTable Methods

        #region Weight matrix & Risk modification

        private void setMainWeightsMatrix(double[,] dMainMatrix)
        { // Sets the main weights matrix
            try
            {
                if ((dMainMatrix == null) || (m_colOptimalPorts.Count == 0)) { m_isSuccessfulCalc = false; return; } // No weights matrix
                m_dWeightsMatrix = dMainMatrix;
                //setRangeOfRiskVals();
            } catch (Exception ex) { m_objErrorHandler.LogInfo(ex); }
        }//setMainWeightsMatrix

        //public int getClosestRiskValIndex(double riskVal)
        //{// Gets the row index of the risk value closest to the parameter point
        //    int minIndex = -1;

        //    for (int iRisks = 0; iRisks < m_colOptimalPorts.Count; iRisks++)
        //    { // Goes through rows in table
        //        if (m_colOptimalPorts[iRisks].Risk == riskVal)
        //            return iRisks;

        //        if (m_colOptimalPorts[iRisks].Risk > riskVal)
        //        { minIndex = iRisks; break; }
        //    } // of for

        //    if (minIndex == 0) return 0;   //first, which is smallest

        //    if (minIndex == -1) return m_colOptimalPorts.Count - 1; //last, which is biggest

        //    if (System.Math.Abs(m_colOptimalPorts[minIndex - 1].Risk - riskVal) < System.Math.Abs(m_colOptimalPorts[minIndex].Risk - riskVal))
        //        return minIndex - 1;

        //    return minIndex;
        //}//getClosestRiskValIndex

        //private void setSecuritiesRiskValues()
        //{ // Sets the updated risk value of the securities (based on optimization results)
        //    for (int iSecs = 0; iSecs < m_colSecurities.Count; iSecs++)
        //        m_colSecurities[iSecs].CovarClass.Risks.Clear(); // Clears previous risks

        //    for (int iPorts = 0; iPorts < m_dWeightsMatrix.GetLength(0); iPorts++)
        //        for (int iSecs = 0; iSecs < m_dWeightsMatrix.GetLength(1); iSecs++)
        //        { // Goes through securities
        //            if (m_dWeightsMatrix[iPorts, iSecs] > 0D)
        //                m_colSecurities[iSecs].CovarClass.Risks.Add(m_colSecurities[iSecs].CovarClass.StandardDeviation * m_dWeightsMatrix[iPorts, iSecs]);
        //            else m_colSecurities[iSecs].CovarClass.Risks.Add(0D);
        //        }
        //}//setSecuritiesRiskValues

        //private void setRangeOfRiskVals()
        //{ // Returns range of risk for current EF calculation
        //    m_pntRangeOfRisks = new System.Drawing.PointF();
        //    m_pntRangeOfRisks.X = (float)(m_colOptimalPorts[m_colOptimalPorts.Count - 1].Risk * 100);
        //    m_pntRangeOfRisks.Y = (float)(m_colOptimalPorts[0].Risk * 100);
        //}//getRangeOfRiskVals

        #endregion Weight matrix & Risk modification

        #region Collections methods

        public void setCollectionsDataForRiskLevel(int iRiskInd)
        { // Sets collections of data for a desired risk level
            try
            {
                m_objColHandler.Sectors.setCollectionWeights();
                m_objColHandler.Markets.setCollectionWeights();
                m_objColHandler.SecTypes.setCollectionWeights();
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//setCollectionsDataForRiskLevel

        public void setSecuritiesWeightsCollection(ISecurities cSecsCol, int iRiskInd)
        { // Sets collection of securities weight items based on the weights matrix
            for (int iSecs = 0; iSecs < cSecsCol.Count; iSecs++)
                cSecsCol[iSecs].Weight = m_dWeightsMatrix[iRiskInd, iSecs];
        }//setSecuritiesWeightsCollection

        #endregion Collections methods

        #endregion Methods

        #region Properties

        public DataTable SecuritiesTable
        { get { return m_dtSecsData; } }//SecuritiesTable

        public Boolean isSuccessful
        {
            get { return m_isSuccessfulCalc; }
            set { m_isSuccessfulCalc = value; }
        }//isSuccessful

        public int PortNumA
        {
            get { return m_iPortPos; }
            set { m_iPortPos = value; }
        }//PortNumA

        public int OrigPortPos
        {
            get { return m_iPortNumOrig; }
            set { m_iPortNumOrig = value; }
        }//OrigPortPos

        public int TangencyPos
        {
            get { return m_iTangencyPortInd; }
            set { m_iTangencyPortInd = value; }
        }//TangencyPos

        public double TangencyPortRisk
        { get { return m_colOptimalPorts[m_iTangencyPortInd].Risk; } }//TangencyPortRisk

        public double[,] WeightsMatrix
        { get { return m_dWeightsMatrix; } }//WeightsMatrix

        public List<cOptimalPort> Portfolios
        { get { return m_colOptimalPorts; } }//Portfolios

        public String EngineError
        { get { return m_strEngineErrors; } }//ScilabError

        //public PointF RiskRange
        //{ get { return m_pntRangeOfRisks; } }//RiskRange

        //public ISecurities SecuritiesCol
        //{
        //    get { return m_colSecurities; }
        //    set { m_colSecurities = value; }
        //}//SecuritiesCol
        
        #endregion Properties

    }//of class
}
