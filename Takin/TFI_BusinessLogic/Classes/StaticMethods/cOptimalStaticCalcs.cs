using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

// Used namespaces
using Cherries.TFI.BusinessLogic.Constraints;
using Cherries.TFI.BusinessLogic.GMath.EF;
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Securities;
using System.IO;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using TFI.BusinessLogic.Interfaces;

namespace Cherries.TFI.BusinessLogic.GMath.StaticMethods
{
    public class cOptimalStaticCalcs : IOptimalStaticCalcs
    {
        public static object lockObject = new object();
        #region Main optimization methods

        public String runOptimizationProcess(out List<double> colRisks, out List<double> colReturns, out double[,] matWeights, double[,] dCovarMatrix, double[] dRatesVector,
            double[] arrUpperBound, double[] arrLowerBound, List<List<double>> colEqualitySecs, double[] arrEqualityVals, int iEqualityNum, List<List<double>> colInequalitySecs,
            double[] arrInequalityVals, int iEps, int iLoopsMax)
        { // Performs the entire optimization process and returns portfolio risk / returns + weights matrix

            // Initialize collections
            colRisks = new List<double>();
            colReturns = new List<double>();

            
            String sSciError = "";
            
            matWeights = new double[0, 0];
           
            CalculationQuery query = new CalculationQuery
                {
                    ArrReturns = dRatesVector,
                    MatCovariance = dCovarMatrix
                };
            lock (lockObject)
            {
                cQLDService.compiler.StandardInput.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(query));
                var res = cQLDService.compiler.StandardOutput.ReadLine();
                CalculationResponse resp = Newtonsoft.Json.JsonConvert.DeserializeObject<CalculationResponse>(res);

                colRisks = resp.colRisks;
                colReturns = resp.colReturns;
                matWeights = resp.matWeights;
                sSciError = resp.errorString;
            }
            return sSciError;
        }//runOptimizationProcess

        private static void multiplyRatesVector(double[] arrRates, double dCurrCoeff)
        { // Multiplies rates array
            for (int iRows = 0; iRows < cQLDService.__R.Length; iRows++)
                cQLDService.__R[iRows] = -(arrRates[iRows] * dCurrCoeff);
        }//multiplyRatesVector

        private static void fillCoeffArray()
        {
            List<double> colCoeffs = new List<double>();
            for (int iItems = 1; iItems < 100; iItems++)
                colCoeffs.Add(iItems / 100D);
            m_arrCoefficients = colCoeffs.ToArray();
        }//fillCoeffArray

        //public static double[] m_arrCoefficients = { 0.0001D, 0.02D, 0.03D, 0.04D,0.05D, 0.06D,0.08D, 0.1D, 
        //                                               0.11D,0.13D,0.16D,0.18D,
        //                                               0.21D,0.23D,0.26D,0.28D,0.29D,
        //                                               0.32D,0.37D,
        //                                               0.45D,0.51D,
        //                                               0.60D,0.72D,
        //                                               0.80D, 1D,
        //                                               1.25D,1.5D,
        //                                               1.86D,
        //                                               1.93D,4D}; // 30 Coefficients
        public static double[] m_arrCoefficients = { 0.01D, 0.02D, 0.03D, 0.04D,0.05D, 0.06D,0.08D, 0.1D,
                                                       0.11D,0.13D,0.16D,0.18D,
                                                       0.21D,0.23D,0.26D,0.28D,0.29D,
                                                       0.32D,0.37D,
                                                       0.45D,0.48D,
                                                       0.51D,0.57D,
                                                       0.61D, 0.68D,
                                                       0.75D,0.79,
                                                       0.86D,
                                                       0.93D,1D}; // 30 Coefficients

        private static double getLastRiskDiff(List<double> colRisks)
        { // Retrieves the last risk difference
            if (colRisks.Count <= 1) return colRisks[0];
            else return colRisks[colRisks.Count - 2] - colRisks[colRisks.Count - 1];
        }//getLastRiskDiff

        public static int calculateQLD(int iVarNum, ref double[] arrWeights)
        { // Performs optimization using QLD
            arrWeights = new double[iVarNum];

            cQLDService.__info = cQLDService.CallMatrixQLD(cQLDService.__Q, cQLDService.__R, cQLDService.__C, cQLDService.__B, cQLDService.__ci, cQLDService.__cs, cQLDService.__me, ref arrWeights, cQLDService.__eps);

            return cQLDService.__info;
        }//calculateQLD

        public static void prepareOptimizationVariables(double[,] dCovarMatrix, double[] dRatesVector, double[] arrUpperBound, double[] arrLowerBound, List<List<double>> colEqualitySecs, 
            double[] arrEqualityVals, int iEqualityNum, List<List<double>> colInequalitySecs,double[] arrInequalityVals, int iEps)
        { // Runs the QLD calculation method

            // SET CONSTRAINT VALUES
            // Bound constraints
            cQLDService.CreateDoubleVector_cs(arrUpperBound);
            cQLDService.CreateDoubleVector_ci(arrLowerBound);

            // Equality constraints
            cQLDService.CreateMatrix_C1(colEqualitySecs);
            cQLDService.CreateVector_B1(arrEqualityVals);
            cQLDService.Create_ME(iEqualityNum);

            // Non-equality constraints
            cQLDService.CreateMatrix_C2(colInequalitySecs);
            cQLDService.CreateVector_B2(arrInequalityVals);

            // Combine constraints
            cQLDService.C_equal_c1_plus_c2(colEqualitySecs, colInequalitySecs);
            cQLDService.B_equal_b1_plus_b2(arrEqualityVals, arrInequalityVals);

            // SET OPTIMIZATION VARIABLES
            // Init matrices
            cQLDService.Create_R(dRatesVector);
            cQLDService.Create_Q(dCovarMatrix);

            // Set matrices
            cQLDService.CreateEmptyMatrix_W();
            cQLDService.minusVector();

            // General
            cQLDService.__eps = iEps;
        }//calculateQLDResults

        #endregion Main optimization methods

        #region General methods

        private void clearMemoryOptimizationVariables()
        { // Clears the variables from the memory
            cQLDService.__B = null;
            cQLDService.__B1 = null;
            cQLDService.__B2 = null;
            cQLDService.__C = null;
            cQLDService.__C1 = null;
            cQLDService.__C2 = null;
            cQLDService.__ci = null;
            cQLDService.__cs = null;
            //cQLDService.__p = null;
            cQLDService.__Q = null;
            //cQLDService.__Q1 = null;
            cQLDService.__R = null;
            cQLDService.__W = null;
            cQLDService.__x = null;
        }//clearMemoryOptimizationVariables

        private double[] getWeightsWithoutLowVals(double[] dWeights)
        { // Returns the same weights matrix, after removing very small values of weights
            for (int iWeights = 0; iWeights < dWeights.Length; iWeights++)
                if (System.Math.Abs(dWeights[iWeights]) < 0.0001) dWeights[iWeights] = 0;
            return dWeights;
        }//getWeightsWithoutLowVals

        #endregion General methods

        #region Optimal portfolio methods

        private void addOptPortfoliosToCollections(double[] arrWeights, double[,] matCovariance, double[] arrReturns, ref List<double> colRisks, ref List<double> colReturns)
        { // Adds calculated portfolios values to collections
            colRisks.Add(getPortfolioRisk(arrWeights, matCovariance));
            colReturns.Add(getPortfolioReturn(arrWeights, arrReturns));
        }//addOptPortfoliosToCollections

        private double getPortfolioRisk(double[] arrWeights, double[,] matCovariance)
        { // Calculates the risk for a given portfolio (mostly optimal)
            double[,] matFinalRisk = cMath.getMultipliedMatrix(cMath.getMultipliedMatrix(cMath.getTransposedVector(arrWeights), matCovariance), cMath.getMatrixVersionOfArr(arrWeights));
            double dFinalRisk = System.Math.Sqrt(Math.Abs(matFinalRisk[0, 0]));
            dFinalRisk = Math.Abs(dFinalRisk);
            return dFinalRisk;
        }//getPortfolioRisk

        //public double getPortfolioRiskNew(double[,] matWeights, cSecurities cColSecs, int iPortPos)
        //{ // Calculates risk of portfolio by summing the risks of the securities
        //    double dFinalVal = 0D;
        //    for (int iSecs = 0; iSecs < matWeights.GetLength(1); iSecs++)
        //        dFinalVal += cColSecs[iSecs].CovarClass.Risks[iPortPos];
        //    return dFinalVal;
        //}//getPortfolioRiskNew

        private double getPortfolioReturn(double[] arrWeights, double[] arrReturns)
        { // Calculates the risk for a given portfolio (mostly optimal)
            double[,] finalRates = cMath.getMultipliedMatrix(cMath.getTransposedVector(arrWeights), cMath.getMatrixVersionOfArr(arrReturns));
            double dFinalReturn = finalRates[0, 0];
            return dFinalReturn;
        }//getPortfolioRisk

        public double getDiversificationValue(double[,] matWeights, ISecurities cColSecs, int iPortPos, double dPortRisk)
        { // Calculates the relevant diversification value
            int iCounter = 0;
            double dNominator = 0D, dDenominator = 0D, dAvgWeight = 0D;

            for (int iSecs = 0; iSecs < matWeights.GetLength(1); iSecs++)
            { // Nominator + weight average calculation
                dNominator += matWeights[iPortPos, iSecs] * cColSecs[iSecs].CovarClass.StDevWeekly;
                dAvgWeight += matWeights[iPortPos, iSecs];

                if (matWeights[iPortPos, iSecs] > 0D) iCounter++;
            }
            dAvgWeight = dAvgWeight / (double)iCounter;
            if (iCounter <= 1) return 0D;

            // DENOMINATOR
            for (int iSecs = 0; iSecs < matWeights.GetLength(1); iSecs++)
                if (matWeights[iPortPos, iSecs] > 0D)
                { // Denominator calculation
                    dDenominator += Math.Pow((matWeights[iPortPos, iSecs] - dAvgWeight), 2);
                }
            dDenominator = dDenominator / (double)(iCounter - 1);
            dDenominator = Math.Sqrt(dDenominator);

            // FINAL VALUE
            double dFinalVal = dNominator / (double)dDenominator;
            //dFinalVal = 1 - (1D / dFinalVal);
            dFinalVal = 1 - dFinalVal;
            dFinalVal = Math.Abs(dFinalVal);
            return dFinalVal;
        }//getDiversificationValue

        #endregion Optimal portfolio methods

    }//of class
}
