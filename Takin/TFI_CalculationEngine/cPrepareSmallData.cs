using Cherries.Classes.GMath;
using Cherries.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TestSciEngine
{
    public class cPrepareSmallData
    {
        cQLDService service;
        #region Methods

        #region Calculation

        #region Data Members

        // Data variables
        public double[] weightResults; // Final results
                                       //private cErrorHandler m_objErrors = new cErrorHandler();

        public static double[] m_arrCoefficients = { 0.001D, 0.005D, 0.008D, 0.01D,
                                                       0.013D, 
                                                       0.021D, 
                                                       0.03D,
                                                       0.051D,
                                                       0.061D,
                                                       0.1D,
                                                       0.3D,
                                                       0.4D,
                                                       0.7D,
                                                       0.83D,1D}; // 15 Coefficients

        //public static double[] m_arrCoefficients = { 0.01D, 0.05D, 0.08D, 0.1D,
        //                                               0.13D, 0.18D,
        //                                               0.21D, 0.29D,
        //                                               0.45D,
        //                                               0.51D,
        //                                               0.61D,
        //                                               0.75D,
        //                                               0.86D,
        //                                               0.93D,1D}; // 15 Coefficients

        //public static double[] m_arrCoefficients = { 0.1D, 0.2D, 0.3D, 0.4D,
        //                                               0.5D, 0.6D,
        //                                               0.7D, 0.8D,
        //                                               0.9D,
        //                                               0.10D,
        //                                               0.11D,
        //                                               0.12D,
        //                                               0.13D,
        //                                               0.14D,0.15D}; // 15 Coefficients

        #endregion Data Members

        #region Constructor

        public cPrepareSmallData()
        {
            service = new Cherries.EF.cQLDService();
            //m_objErrors = cErrors;
            //cQLDService.LoadLibrary(@"C:\Users\liorp\Documents\Visual Studio 2012\projects\TestSciEngine\msvcr100d.dll");
        }//constructor

        #endregion Constructor
        public String runOptimizationProcess(out List<double> colRisks, out List<double> colReturns, out double[,] matWeights, double[,] dCovarMatrix, double[] dRatesVector)
        { // Performs the entire optimization process and returns portfolio risk / returns + weights matrix

            // Initialize collections
            colRisks = new List<double>();
            colReturns = new List<double>();

            // Initialize variables
            int iArrCoeffPos = 0;
            String sSciError = "";
            double[] arrWeights = new double[dRatesVector.Length];
            matWeights = new double[0, 0];
            prepareOptimizationVariables(dCovarMatrix, dRatesVector, dRatesVector.Length);

            double[] arrTimes = new double[m_arrCoefficients.Length];

            //cProperties.Watch.Reset();
            do
            { // Calculates optimal portfolios
                // Calculation
                multiplyRatesVector(dRatesVector, m_arrCoefficients[iArrCoeffPos]);

                //cProperties.Watch.Start();
                //calculateQLD(dRatesVector.Length, ref arrWeights);
                service.QLD();
                //arrTimes[iArrCoeffPos] = cProperties.Watch.Elapsed.TotalMilliseconds;

                // Error handling
                sSciError = service.LastError();
                if (sSciError != "")
                { // Found a calculation error
                    if (colReturns.Count > 5) continue;
                    else return sSciError;
                }

                // Append to collections and advance loop
                //service.__x = null;
                service.__x = getWeightsWithoutLowVals(service.__x);
                service.Add_X_To_W();
                addOptPortfoliosToCollections(service.__x, dCovarMatrix, dRatesVector, ref colRisks, ref colReturns);

                iArrCoeffPos++;
            } while (iArrCoeffPos < m_arrCoefficients.Length);

            // Unable to calculate
            if (colReturns.Count == 0) return "Failed to calculate Efficient Frontier";

            // Results
            matWeights = cQLDService.get1dTo2dArr(service.getMatrix_W(), new System.Drawing.Size(dRatesVector.Length, colReturns.Count));

            //writeTimesToFile(arrTimes);
            clearMemoryOptimizationVariables();

            return sSciError;
        }//runOptimizationProcess
        public void prepareOptimizationVariables(double[,] matCovar, double[] arrReturns, int iArrLength)
        {
            try
            {

                setConstsForEngine(iArrLength);

                // Init variables
                service.CreateEmptyMatrix_W();
                service.Create_R(arrReturns);

                // Data points (rates)    
                service.Create_Q(matCovar);
                //cQLDService.minusVector();
                //service.multiplyRatesVector(arrReturns, currentCoeff);
                // Run Optimization
                service.multiply_matrix_coefficient(1);
                //multiplyRatesVector(arrReturns, currentCoeff);
                //service.QLD();

                // Capture errors
                //weightResults = service.__x;
            } catch (Exception ex) {
                //m_objErrors.LogInfo(ex);
            }
        }//runMainCalculation
        private void addOptPortfoliosToCollections(double[] arrWeights, double[,] matCovariance, double[] arrReturns, ref List<double> colRisks, ref List<double> colReturns)
        { // Adds calculated portfolios values to collections
            colRisks.Add(getPortfolioRisk(arrWeights, matCovariance));
            colReturns.Add(getPortfolioReturn(arrWeights, arrReturns));
        }//addOptPortfoliosToCollections

        private double getPortfolioRisk(double[] arrWeights, double[,] matCovariance)
        { // Calculates the risk for a given portfolio (mostly optimal)
            double[,] matFinalRisk = cMath.getMultipliedMatrix(cMath.getMultipliedMatrix(cMath.getTransposedVector(arrWeights), matCovariance), cMath.getMatrixVersionOfArr(arrWeights));
            double dFinalRisk = System.Math.Sqrt(matFinalRisk[0, 0]);
            if (double.IsNaN(dFinalRisk)) dFinalRisk = 0;
            dFinalRisk = Math.Abs(dFinalRisk);
            return dFinalRisk;
        }//getPortfolioRisk

        private static double getPortfolioReturn(double[] arrWeights, double[] arrReturns)
        { // Calculates the risk for a given portfolio (mostly optimal)
            double[,] finalRates = cMath.getMultipliedMatrix(cMath.getTransposedVector(arrWeights), cMath.getMatrixVersionOfArr(arrReturns));
            double dFinalReturn = finalRates[0, 0];
            return dFinalReturn;
        }//getPortfolioRisk
        private double[] getWeightsWithoutLowVals(double[] dWeights)
        { // Returns the same weights matrix, after removing very small values of weights
            for (int iWeights = 0; iWeights < dWeights.Length; iWeights++)
                if (System.Math.Abs(dWeights[iWeights]) < 0.0001) dWeights[iWeights] = 0;
            return dWeights;
        }//getWeightsWithoutLowVals

        private void multiplyRatesVector(double[] arrRates, double dCurrCoeff)
        { // Multiplies rates array
            for (int iRows = 0; iRows < service.__R.Length; iRows++)
                service.__R[iRows] = -(arrRates[iRows] * dCurrCoeff);
        }//multiplyRatesVector

        private void clearMemoryOptimizationVariables()
        { // Clears the variables from the memory
            service.__B = null;
            service.__B1 = null;
            service.__B2 = null;
            service.__C = null;
            service.__C1 = null;
            service.__C2 = null;
            service.__ci = null;
            service.__cs = null;
            //cQLDService.__p = null;
            service.__Q = null;
            //cQLDService.__Q1 = null;
            service.__R = null;
            service.__W = null;
            service.__x = null;
        }//clearMemoryOptimizationVariables
        #endregion Calculation

        #region Constraints

        // Constraint Strings
        private List<List<double>> m_doubleEqualitySecs = new List<List<double>>();   //  --c1--      List containing securities matrix for equality constraints
        private double[] m_doubleEqualityVals = { 1 };   //  --b1--      List containing values of equality constraints

        private int m_intEqualityNum = 1;    // --me--   String containing the number of equality constraints 

        private List<List<double>> m_doubleRangeSecs;   // --c2--  List containing securities matrix for range constraints
        private double[] m_doubleRangeVals;         // --b2--  List containing values of range constraints

        private void setConstsForEngine(int iVarSize)
        { // Sets constraint variables for Engine calculation

            // Boundaries constraints
            double[] m_arrSecsUpperBound = new double[iVarSize];
            double[] m_arrSecsLowerBound = new double[iVarSize];
            for (int i = 0; i < iVarSize; i++)
            {
                m_arrSecsUpperBound[i] = 1;
                m_arrSecsLowerBound[i] = 0;
            }

            service.CreateDoubleVector_cs(m_arrSecsUpperBound);
            service.CreateDoubleVector_ci(m_arrSecsLowerBound);

            setConstraintVariables(iVarSize);
            // Equality constraints
            service.CreateMatrix_C1(m_doubleEqualitySecs);
            service.CreateVector_B1(m_doubleEqualityVals);
            service.Create_ME(m_intEqualityNum);

            // Non-equality constraints
            service.CreateMatrix_C2(m_doubleRangeSecs);
            service.CreateVector_B2(m_doubleRangeVals);

            // Combine constraints
            service.C_equal_c1_plus_c2(m_doubleEqualitySecs, m_doubleRangeSecs);
            service.B_equal_b1_plus_b2(m_doubleEqualityVals, m_doubleRangeVals);
        }//setConstsForEngine

        private void setConstraintVariables(int iVarSize)
        {
            m_doubleEqualitySecs.Clear();
            List<double> lstEqualitySecs = new List<double>();
            for (int iSecs = 0; iSecs < iVarSize; iSecs++)
                lstEqualitySecs.Add(1);
            m_doubleEqualitySecs.Add(lstEqualitySecs);

            

            m_doubleRangeSecs = new List<List<double>>();
            //List<double> lstRangeSecs = new List<double>();
            //m_doubleRangeSecs.Add(lstRangeSecs);
            m_doubleRangeVals = new double[0];
        }//setConstraintVariables

        #endregion Constraints

        #endregion Methods

    }//class
}
