using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Linq;

// Used namespaces
using Cherries.TFI.BusinessLogic.Collections;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.DataManagement.StaticMethods;
using Cherries.TFI.BusinessLogic.GMath.StaticMethods;
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Portfolio;
using Cherries.TFI.BusinessLogic.Securities;
using TFI.BusinessLogic.Interfaces;
using Cherries.TFI.BusinessLogic.Optimization;
using Cherries.Models.App;

namespace Cherries.TFI.BusinessLogic.GMath
{
    public class cCovarCorrelHandler : IDisposable, ICovarCorrelHandler
    {

        #region Data Members

        // Main variables
        private IPortfolioBL m_objPortfolio; // Portfolio class pointer
        private IRateHandler m_objRateHandler; // Rate calculation class pointer
        private IErrorHandler m_objErrorHandler; // Error handler class
        private ICollectionsHandler m_objColHandler; // Collections handler class
        private bool isDisposed = false; // indicates if Dispose has already been called

        // Data variables
        private DataTable m_dtCovarOutput = null; // Covariance DataTable
        private double[,] m_dCovarMatrix = null; // Covariance final matrix

        #endregion Data Members

        #region Constructors, Initialization & Destructor

        public cCovarCorrelHandler(IPortfolioBL cPort)
        {
            // Init variables
            m_objPortfolio = cPort;
            m_objErrorHandler = m_objPortfolio.cErrorLog;
            m_objColHandler = m_objPortfolio.ColHandler;
            m_objRateHandler = m_objPortfolio.Classes.RatesHandler;

            m_dtCovarOutput = getOutputDataStruct(); // Output data structure
        }//constructor

        ~cCovarCorrelHandler()
        { Dispose(false); }//destructor

        protected void Dispose(bool disposing)
        { // clearing class variables
            if (disposing)
            { //clean up managed resources
                m_objRateHandler = null;
                m_objErrorHandler = null;
                m_objColHandler = null;
                if (m_dtCovarOutput != null) m_dtCovarOutput.Dispose();
            }
            isDisposed = true;
        }//Dispose

        public void Dispose()
        { // indicates it was NOT called by the Garbage collector
            Dispose(true);
            GC.SuppressFinalize(this); // no need to do anything, stop the finalizer from being called
        }//Dispose

        public void clearCalcData()
        { // Clears relevant calculation data
            if (m_dtCovarOutput != null) m_dtCovarOutput.Clear();
            m_dCovarMatrix = null;
        }//clearCalcData

        #endregion Constructors, Initialization & Destructor

        #region Methods

        #region New methods

        public double[,] calcRiskAndCovariance(cDateRange drPeriod, ref ISecurities cSecsCol, Boolean isMatrix, Boolean isBacktest)
        { // Runs the main covariance calculation (including covariance matrix)
            ///  The matrix's first dimension represents the variables (securities)C:\Users\uriel\Documents\Applications\Cherries\Classes\
            ///  The matrix's second dimension represents the observations (history)
            try
            {
                if (isBacktest)
                    for (int iSer = 0; iSer < cSecsCol.Count; iSer++)
                        cSecsCol[iSer].RatesClass.setUpdatedRateData(drPeriod);
                else
                    for (int iSer = 0; iSer < cSecsCol.Count; iSer++)
                        cSecsCol[iSer].RatesClass.setFinalReturnFromServer(drPeriod);

                cSecsCol = cSecsCol.getListOfActiveSecs();
                for (int iSecs = 0; iSecs < cSecsCol.Count; iSecs++)
                    cSecsCol[iSecs].CovarClass.setCalculationStatistics(isBacktest);

                cSecsCol = cSecsCol.getListOfActiveSecs();
                if (isMatrix)
                    calculateNewCovarianceMatrix(cSecsCol, isBacktest);
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
            return m_dCovarMatrix;
        }//runMainCalculation


        //private void prepareSecurityStatistics(ISecurities cSecsCol)
        //{ // Prepares statistics calculations (st-Dev / avg) for each security
        //    cProperties.QaPos = 0; // Restart Qa position
        //    for (int iSecs = 0; iSecs < cSecsCol.Count; iSecs++)
        //        cSecsCol[iSecs].CovarClass.setCalculationStatistics();
        //}//prepareSecurityStatistics

        private void calculateNewCovarianceMatrix(ISecurities cSecsCol, Boolean isBacktesting)
        { // Calculates the covariance matrix 

            try
            {
                List<PriceReturn> colRatesI = new List<PriceReturn>();
                List<PriceReturn> colRatesJ = new List<PriceReturn>();

                m_dCovarMatrix = new double[cSecsCol.Count, cSecsCol.Count]; // Empty matrix

                for (int i = 0; i < cSecsCol.Count; i++)
                {
                    for (int j = 0; j < cSecsCol.Count; j++)
                    { // Fills covariance matrix
                        if (i <= j)
                        {
                            colRatesI = (isBacktesting) ? cSecsCol[i].RatesClass.BacktestingReturns : cSecsCol[i].RatesClass.PriceReturns;
                            colRatesJ = (isBacktesting) ? cSecsCol[j].RatesClass.BacktestingReturns : cSecsCol[j].RatesClass.PriceReturns;

                            m_dCovarMatrix[i, j] = cBasicStaticCalcs.getNewCovarianceValue(colRatesI, cSecsCol[i].CovarClass.Average, colRatesJ,
                                cSecsCol[j].CovarClass.Average);
                            if (i != j)
                                m_dCovarMatrix[j, i] = m_dCovarMatrix[i, j];
                        }
                    }//main for
                }
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//calculateCovarianceMatrix

        private DataTable getCovarMatTableStruct(ISecurities cSecsCol)
        { // Creates structure for covariance matrix table
            DataTable dtFinal = new DataTable();
            dtFinal.Columns.Add(new DataColumn("strName", typeof(String)));
            for (int iSecs = 0; iSecs < cSecsCol.Count; iSecs++)
                if (cSecsCol[iSecs].Weight > 0) 
                    dtFinal.Columns.Add(new DataColumn(cSecsCol[iSecs].Properties.PortSecurityId, typeof(String)));

            return dtFinal;
        }//getCovarMatTableStruct

        #endregion New methods

        #region Report Handling

        public void reportCovarianceMatrix(ISecurities cSecsCol, DataTable dtResults, cOptimalPort cPort, double dEquity, Boolean isBacktesting)
        { // Writes a report (to csv file) of covariance matrix 

            try
            {
                createCovariancePairResults(cSecsCol, "AAOI", "EXEL");
                createCovariancePairResults(cSecsCol, "EXEL", "EXEL");
                createCovariancePairResults(cSecsCol, "WTW", "AAMC");
                createCovariancePairResults(cSecsCol, "AAOI", "AAOI");
                createCovariancePairResults(cSecsCol, "EVI", "NOVT");
                createCovariancePairResults(cSecsCol, "NVDA", "NOVT");

                StreamWriter writer = new StreamWriter("C:\\TempFiles\\portfolioReport.csv");

                writer.WriteLine("תיק אופטימאלי");
                writer.WriteLine();
                writer.WriteLine("תשואה, " + cGeneralFunctions.getFormatPercentFromDbl(cPort.Return, 2));
                writer.WriteLine("סיכון, " + cGeneralFunctions.getFormatPercentFromDbl(cPort.Risk, 2));
                writer.WriteLine("יחס תשואה לסיכון, " + cGeneralFunctions.getFormatPercentFromDbl(cPort.RateToRisk, 2));
                writer.WriteLine("סכום השקעה, " + cGeneralFunctions.getFormatPercentFromDbl(dEquity, 2));
                writer.WriteLine();
                writer.WriteLine();
                writer.WriteLine("מטריצת קוואריאנס");

                DataTable dtCovarMat = getCovarianceMatTable(cSecsCol, isBacktesting);
                addTableToStream(dtCovarMat, writer, true);

                writer.WriteLine();
                writer.WriteLine();
                writer.WriteLine("טבלה סופית");
                addTableToStream(dtResults, writer, true);

                //cClientStaticMethods.writeDataTableToCsv(dtCovarMat, "C:\\TempFiles\\covarMatrix.csv", true);
                writer.Flush();
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//reportCovarianceMatrix

        private DataTable getCovarianceMatTable(ISecurities cSecsCol, Boolean isBacktesting)
        { // Returns a datatble containing a shortened covariance matrix
            DataTable dtCovarForReport = getCovarMatTableStruct(cSecsCol);
            DataRow drCurrLine;
            int iColPos = 0;
            List<PriceReturn> colRatesI = new List<PriceReturn>();
            List<PriceReturn> colRatesJ = new List<PriceReturn>();

            for (int i = 0; i < cSecsCol.Count; i++)
            {
                iColPos = 1;
                drCurrLine = dtCovarForReport.NewRow();
                for (int j = 0; j < cSecsCol.Count; j++)
                { // Fills covariance matrix
                    if ((cSecsCol[i].Weight > 0) && (cSecsCol[j].Weight > 0))
                    { // Minimized covariance matrix for report
                        if ((cSecsCol[i].Properties.SecuritySymbol == "NOVT") && (cSecsCol[j].Properties.SecuritySymbol == "NVDA"))
                            Console.Write("");
                        if ((cSecsCol[i].Properties.SecuritySymbol == "NVDA") && (cSecsCol[j].Properties.SecuritySymbol == "NOVT"))
                            Console.Write("");
                        if ((cSecsCol[i].Properties.SecuritySymbol == "AAOI") && (cSecsCol[j].Properties.SecuritySymbol == "AAOI"))
                            Console.Write("");


                        colRatesI = (isBacktesting) ? cSecsCol[i].RatesClass.BacktestingReturns : cSecsCol[i].RatesClass.PriceReturns;
                        colRatesJ = (isBacktesting) ? cSecsCol[j].RatesClass.BacktestingReturns : cSecsCol[j].RatesClass.PriceReturns;

                        drCurrLine[iColPos] = cBasicStaticCalcs.getNewCovarianceValue(colRatesI, cSecsCol[i].CovarClass.Average, colRatesJ,
                            cSecsCol[j].CovarClass.Average);
                        iColPos++;
                    }
                }//main for
                if (iColPos > 1)
                { // Writes line to report table
                    drCurrLine[0] = cSecsCol[i].Properties.SecuritySymbol;
                    dtCovarForReport.Rows.Add(drCurrLine);
                }
            }

            return dtCovarForReport;
        }//getCovarianceMatTable

        private void addTableToStream(DataTable sourceTable, StreamWriter writer, bool includeHeaders)
        { // Inserts a given datatable to a stream writer (for report)
            if(includeHeaders)
            { // Writes headers
                IEnumerable<String> headerValues = sourceTable.Columns
                    .OfType<DataColumn>()
                    .Select(column => quoteValue(column.ColumnName));

                writer.WriteLine(String.Join(",", headerValues));
            }

            IEnumerable<String> items = null;

            foreach (DataRow row in sourceTable.Rows)
            { // Writes data
                items = row.ItemArray.Select(o => quoteValue(o?.ToString() ?? String.Empty));
                writer.WriteLine(String.Join(",", items));
            }
        }//addTableToStream

        private static string quoteValue(string value)
        {
            return String.Concat("\"",
            value.Replace("\"", "\"\""), "\"");
        }//quoteValue

        private void createCovariancePairResults(ISecurities cSecsCol, String strSymbol1, String strSymbol2)
        { // Creates a report of pairs of securities (to determine which 
            try
            {
                StreamWriter writer = new StreamWriter("C:\\TempFiles\\pair" + strSymbol1 + "_" + strSymbol2 + ".csv");
                
                writer.WriteLine("חישוב קוואריאנס");
                writer.WriteLine();

                double? dFinalVal = 0D; int iCounter = 0;
                for (int i = 0; i < cSecsCol.Count; i++)
                { // Goes through securities
                    if (cSecsCol[i].Properties.SecuritySymbol != strSymbol1) continue;
                    for (int j = 0; j < cSecsCol.Count; j++)
                        if(cSecsCol[j].Properties.SecuritySymbol == strSymbol2)
                        { // Pair found
                            writer.WriteLine("Date, " + strSymbol1 + "_return, " + strSymbol1 + "_avg, " + strSymbol1 + ": return - avg, " + strSymbol2 + "_return, " + strSymbol2 + "_avg, " + strSymbol2 + ": return - avg, Multiplication");
                            var query = from sec1 in cSecsCol[i].RatesClass.BacktestingReturns
                                        join sec2 in cSecsCol[j].RatesClass.BacktestingReturns
                                             on sec1.dtDate equals sec2.dtDate
                                        select new
                                        {
                                            sec1.dtDate,
                                            Return1 = sec1.dMiniReturn,
                                            Return2 = sec2.dMiniReturn
                                        };

                            foreach (var item in query)
                            { // Goes through all items in query
                                writer.WriteLine(item.dtDate.ToString() + "," + item.Return1 + "," + cSecsCol[i].CovarClass.Average + "," +
                                    (item.Return1 - cSecsCol[i].CovarClass.Average).ToString() + "," +
                                    item.Return2 + "," + cSecsCol[j].CovarClass.Average + "," + (item.Return2 - cSecsCol[j].CovarClass.Average).ToString() + "," +

                                    ((item.Return1 - cSecsCol[i].CovarClass.Average) * (item.Return2 - cSecsCol[j].CovarClass.Average)));


                                dFinalVal += ((item.Return1 - cSecsCol[i].CovarClass.Average) * (item.Return2 - cSecsCol[j].CovarClass.Average));
                                iCounter++;
                            }



                            //for (int iRows = 0; (iRows < cSecsCol[i].RatesClass.PriceReturns.Count) && (iRows < cSecsCol[j].RatesClass.PriceReturns.Count); iRows++)
                            //{ // Calculates sum of covariance value
                            //    writer.WriteLine(cSecsCol[i].RatesClass.PriceReturns[iRows].dtDate.ToString() + "," + cSecsCol[i].RatesClass.PriceReturns[iRows].dReturn + "," + cSecsCol[i].RatesClass.ExpectedReturnAvg + "," +
                            //        (cSecsCol[i].RatesClass.PriceReturns[iRows].dReturn - cSecsCol[i].RatesClass.ExpectedReturnAvg).ToString() + "," +
                            //        cSecsCol[j].RatesClass.PriceReturns[iRows].dReturn + "," + cSecsCol[j].RatesClass.ExpectedReturnAvg + "," +
                            //        (cSecsCol[j].RatesClass.PriceReturns[iRows].dReturn - cSecsCol[j].RatesClass.ExpectedReturnAvg).ToString() + "," +

                            //        ((cSecsCol[i].RatesClass.PriceReturns[iRows].dReturn - cSecsCol[i].RatesClass.ExpectedReturnAvg) * (cSecsCol[j].RatesClass.PriceReturns[iRows].dReturn - cSecsCol[j].RatesClass.ExpectedReturnAvg)));


                            //    dFinalVal += ((cSecsCol[i].RatesClass.PriceReturns[iRows].dReturn - cSecsCol[i].RatesClass.ExpectedReturnAvg) * (cSecsCol[j].RatesClass.PriceReturns[iRows].dReturn - cSecsCol[j].RatesClass.ExpectedReturnAvg));
                            //    iCounter++;
                            //}
                        }
                } // Main for

                writer.WriteLine();
                writer.WriteLine();
                writer.WriteLine("Final Result (Covariance)");
                if ((iCounter == 1) || (dFinalVal == 0D))
                    writer.WriteLine("Error in calculation");
                else
                    //writer.WriteLine((dFinalVal / ((double)iCounter - 1)).ToString());
                    writer.WriteLine((dFinalVal / ((double)51)).ToString());

                writer.Flush();
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//createCovariancePairsResults

        #endregion Report Handling

        #region Final Table

        public void setFinalTbl(cSecurities cSecsCol)
        { // Returns the final Table with the covariance / correlation values (for all securities)
            try
            {
                if (m_dtCovarOutput == null) m_dtCovarOutput = getOutputDataStruct();
                else m_dtCovarOutput.Clear();

                StringBuilder sb = new StringBuilder(); 
                sb.AppendLine("SecId1, SecId2, Covariance"); 

                if ((cSecsCol.Count != m_dCovarMatrix.GetLength(0)) || (cSecsCol.Count != m_dCovarMatrix.GetLength(1))) return; // Matrix doesn't match securities - abort

                for (int i = 0; i < m_dCovarMatrix.GetLength(0); i++)
                    for (int j = 0; j < m_dCovarMatrix.GetLength(1); j++)
                    {
                        if (cProperties.isQARequested) sb.AppendLine(cSecsCol[i].Properties.PortSecurityId + ", " + cSecsCol[j].Properties.PortSecurityId + ", " + m_dCovarMatrix[i, j].ToString());
                        m_dtCovarOutput.Rows.Add(cSecsCol[i].Properties.SecuritySymbol + " (" + cSecsCol[i].Properties.MarketName + ")",
                            cSecsCol[j].Properties.SecuritySymbol + " (" + cSecsCol[j].Properties.MarketName + ")", m_dCovarMatrix[i, j]);
                    }

                if (cProperties.isQARequested) File.WriteAllText(cProperties.DataFolder + "\\" + cGeneralFunctions.getDateFormatStr(DateTime.Today, "_") +"_CovarMatrix.csv", sb.ToString());
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//getFinalTbl

        private DataTable getOutputDataStruct()
        { // Creates the structure of the output datatable 
            DataTable dtNewTbl = new DataTable();
            dtNewTbl.Columns.Add(new DataColumn("strSec1", typeof(String)));
            dtNewTbl.Columns.Add(new DataColumn("strSec2", typeof(String)));
            dtNewTbl.Columns.Add(new DataColumn("dblVal", typeof(double)));
            return dtNewTbl;
        }//getOutputDataStruct

        #endregion Final Table

        #endregion Methods

        #region Properties

        public DataTable CovarTable
        {
            get { return m_dtCovarOutput; }
            set { m_dtCovarOutput = value; }
        }//CovarTable

        public double[,] CovarMatrix
        { 
            get { return m_dCovarMatrix; }
            set { m_dCovarMatrix = value; }
        }//CovarMatrixTable

        #endregion Properties

    }// of class
}
