using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;

// Used namespaces

namespace Cherries.Classes.GMath
{
    public static class cMath
    {

        #region Methods

        #region Basic operations

        public static double getDataTblFldAvg(DataTable dtOrigData, String sValFldName)
        { // Returns average value of a data table field
            double dFinalVal = 0D;
            for (int iRows = 0; iRows < dtOrigData.Rows.Count; iRows++)
                dFinalVal += Convert.ToDouble(dtOrigData.Rows[iRows][sValFldName]);
            return dFinalVal / (double)dtOrigData.Rows.Count;
        }//getDataTblAvg

        public static double getDataTblMultFldAvg(DataTable dtFld1, String sFldVal1,
            DataTable dtFld2, String sFldVal2)
        { // Returns average of field multiplication, out of 2 seperate fields in 2 seperate Datatables 
            double dFinalVal = 0D;
            if (!(dtFld1.Rows.Count == dtFld2.Rows.Count)) return 0;

            for (int iRows = 0; iRows < dtFld1.Rows.Count; iRows++)
                dFinalVal += Convert.ToDouble(dtFld1.Rows[iRows][sFldVal1]) *
                    Convert.ToDouble(dtFld2.Rows[iRows][sFldVal2]);
            return dFinalVal / (double)dtFld1.Rows.Count;
        }//getDataTblDblFldAvg

        public static double getDataTblFldVariance(DataTable dtOrigData, String sValFldName, double dAvgVal)
        { // Returns Standard deviation value of a data table field
            double dFinalVal = 0D;
            for (int iRows = 0; iRows < dtOrigData.Rows.Count; iRows++)
                dFinalVal += System.Math.Pow(Convert.ToDouble(dtOrigData.Rows[iRows][sValFldName]) - dAvgVal, 2);
            //dFinalVal = (dFinalVal / (double)dtOrigData.Rows.Count) * cProperties.getCurrentScaling(); // Variance + annualization
            dFinalVal = (dFinalVal / (double)(dtOrigData.Rows.Count - 1)); // Variance + annualization

            return dFinalVal;
            //return System.Math.Sqrt(dFinalVal);

            //double dYears = (cProperties.CalcRange.EndDate - cProperties.CalcRange.StartDate).Days / 365D;
            //double dFinalVal = 0D;
            //for (int iRows = 0; iRows < dtOrigData.Rows.Count; iRows++)
            //    dFinalVal += System.Math.Pow(Convert.ToDouble(dtOrigData.Rows[iRows][sValFldName]) - dAvgVal, 2);
            //dFinalVal = (dFinalVal / (double)dtOrigData.Rows.Count) * Math.Sqrt(cProperties.getCurrentScaling() * dYears); // Variance + annualization
            //return System.Math.Sqrt(dFinalVal);
        }//getDataTblFldStDev
                
        public static Boolean isNumericText(String strVal, Boolean allowNegative)
        { // Checks whether a given text is numeric
            String strCharFormat = "^[0-9.," + ((allowNegative) ? "" : "-") + "]+$"; double dTmp;
            if ((Regex.IsMatch(strVal, @strCharFormat)) && (Double.TryParse(strVal, out dTmp))) return true;
            return false;
        }//isNumericText

        public static bool isNaN(double d)
        { return (d != d); }//IsNaN

        #endregion Basic operations

        #region Weighted operations

        public static double getDataTblWMAFldAvg(DataTable dtOrigData, String sValFldName, String strWeightFldName)
        { // Returns average value of a data table field
            double dFinalVal = 0D;
            for (int iRows = 0; iRows < dtOrigData.Rows.Count; iRows++)
                dFinalVal += Convert.ToDouble(dtOrigData.Rows[iRows][sValFldName]) * Convert.ToDouble(dtOrigData.Rows[iRows][strWeightFldName]);
            return dFinalVal;
        }//getDataTblAvg

        public static double getDataTblFldWMAStDev(DataTable dtOrigData, String sValFldName, String strWeightFldName, double dAvgVal)
        { // Returns Standard deviation value of a data table field
            double dFinalVal = 0D;
            for (int iRows = 0; iRows < dtOrigData.Rows.Count; iRows++)
                dFinalVal += System.Math.Pow(Convert.ToDouble(dtOrigData.Rows[iRows][sValFldName]) - dAvgVal, 2) * Convert.ToDouble(dtOrigData.Rows[iRows][strWeightFldName]);
            //dFinalVal = dFinalVal * cProperties.getCurrentScaling(); // Variance + annualization
            dFinalVal = dFinalVal * dtOrigData.Rows.Count; // Variance + annualization
            return System.Math.Sqrt(dFinalVal);
        }//getDataTblFldStDev

        #endregion Weighted operations

        #region Min-max operations

        public static double getDataMinVal(int iFldInd, double[,] dWorkMatrix)
        { // Sets minimal and maximal values from the given data
            double currVal = 0;
            double dMinVal = double.MaxValue;

            int colLenRun = dWorkMatrix.GetLength(0) - 1; // Length of columns to run
            int iColStart = 0; // Starting col
            if (iFldInd > -1)
            { // (considers single col case)
                iColStart = iFldInd;
                colLenRun = iFldInd;
            }

            for (int iRows = 0; iRows <= dWorkMatrix.GetLength(1) - 1; iRows++)
                for (int iCols = iColStart; iCols <= colLenRun; iCols++)
                {
                    //currVal = dWorkMatrix[iRows, iCols];
                    currVal = dWorkMatrix[iCols, iRows];
                    if (currVal < dMinVal) dMinVal = currVal;
                }
            return dMinVal;
        }//setDataMinMaxVals

        public static double getDataMaxVal(int iFldInd, double[,] dWorkMatrix)
        { // Sets minimal and maximal values from the given data
            double currVal = 0;
            double dMaxVal = double.MinValue;

            int colLenRun = dWorkMatrix.GetLength(0) - 1; // Length of columns to run
            int iColStart = 0; // Starting col
            if (iFldInd > -1)
            { // (considers single col case)
                iColStart = iFldInd;
                colLenRun = iFldInd;
            }

            for (int iRows = 0; iRows <= dWorkMatrix.GetLength(1) - 1; iRows++)
                for (int iCols = iColStart; iCols <= colLenRun; iCols++)
                {
                    //currVal = dWorkMatrix[iRows, iCols];
                    currVal = dWorkMatrix[iCols, iRows];
                    if (currVal > dMaxVal) dMaxVal = currVal;
                }
            return dMaxVal;
        }//setDataMinMaxVals

        public static int getArrMaxValIndex(double[] arrData)
        { // gets the index of the maximal found value
            double dMaxVal = double.MinValue;
            int iInd = -1;
            for (int iVals = 0; iVals < arrData.Length; iVals++)
                if (arrData[iVals] > dMaxVal)
                { dMaxVal = arrData[iVals]; iInd = iVals; }
            return iInd;
        }//getArrMaxValIndex

        #endregion Min-max operations

        #region Financial operations

        public static double[] getSharpeRatioArr(double[] arrRisk, double[] arrRate, double dRiskFree)
        { // returns array of sharpe ratio values
            if ((arrRate == null) || (arrRisk == null)) return null;
            if (arrRate.Length != arrRisk.Length) return null;

            double[] arrSharpe = new double[arrRisk.Length];
            for (int iVals = 0; iVals < arrSharpe.Length; iVals++)
                arrSharpe[iVals] = (arrRate[iVals] - dRiskFree) / arrRisk[iVals]; // Sharpe ratio calculation
            return arrSharpe;
        }//getSharpeRatioArr

        //public static double[] getSharpeRatioArr(List<double> arrRisk, List<double> arrRate, double dRiskFree)
        
        #endregion Financial operations

        #region Matrix multiplication

        //----------------------------------------------------------------------------------------------------------
        //                                             sub 2.1
        //----------------------------------------------------------------------------------------------------------

        public static double[,] getMultipliedMatrix(double[,] matA, double[,] matB)
        { // Gets a multiplied matrix result
            if ((matA == null) || (matB == null)) return null;
            if (matA.GetLength(1) != matB.GetLength(0)) return null; // Must be equal sizes

            double[,] matFinal = new double[matA.GetLength(0), matB.GetLength(1)];
            for (int i = 0; i < matFinal.GetLength(0); i++)
                for (int j = 0; j < matFinal.GetLength(1); j++)
                {
                    matFinal[i, j] = 0;
                    for (int k = 0; k < matA.GetLength(1); k++) // OR k<b.GetLength(0)
                        matFinal[i, j] = matFinal[i, j] + matA[i, k] * matB[k, j];
                }

            return matFinal;
        }//getMultipliedMatrix

        //----------------------------------------------------------------------------------------------------------
        //                                             sub 2.3
        //----------------------------------------------------------------------------------------------------------

        public static double[,] getMatrixVersionOfArr(double[] arrVals)
        { // Transforms vector to matrix (only by presentation)
            if (arrVals == null) return null;
            double[,] matFinal = new double[arrVals.Length, 1];
            for (int i = 0; i < arrVals.Length; i++) matFinal[i, 0] = arrVals[i];
            return matFinal;
        }//getMatrixVersionOfArr

        //----------------------------------------------------------------------------------------------------------
        //                                             sub 2.2
        //----------------------------------------------------------------------------------------------------------

        public static double[,] getTransposedVector(double[] arrVals)
        { // Returns matrix of transposed vector
            if (arrVals == null) return null;
            double[,] matFinal = new double[1, arrVals.Length];
            for (int i = 0; i < arrVals.Length; i++) matFinal[0, i] = arrVals[i];
            return matFinal;
        }//getTransposedVector

        #endregion Matrix multiplication

        #endregion Methods

    }// of class
}
