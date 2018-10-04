using System;
using System.Collections.Generic;
using System.Text;

namespace Cherries.TFI.BusinessLogic.General
{
    public static class cArrayHandler
    {

        #region Methods

        #region Jagged arrays

        public static int getJaggedArrMaxDimSize(double[][] dOrigArr)
        { // Gets maximum size of 2nd dimension in jagged array
            int iMax2ndDim = 0; // maximal value of second dimension
            for (int iD = 0; iD < dOrigArr.GetLength(0); iD++)
                if (dOrigArr[iD].GetLength(0) > iMax2ndDim)
                    iMax2ndDim = dOrigArr[iD].GetLength(0);
            return iMax2ndDim;
        }//getJaggedArrMaxDimSize

        public static double[,] convertJaggedArrTo2DArr(double[][] dOrigArr)
        { // Converts jagged array to 2-dimensional normal array
            double[,] finalArr = new double[dOrigArr.GetLength(0), getJaggedArrMaxDimSize(dOrigArr)];
            for (int iD1 = 0; iD1 < finalArr.GetLength(0); iD1++)
                for (int iD2 = 0; iD2 < finalArr.GetLength(1); iD2++)
                    if (iD2 < dOrigArr[iD1].Length)
                        finalArr[iD1, iD2] = dOrigArr[iD1][iD2];
            return finalArr;
        }//convertJaggedArrTo2DArr

        #endregion Jagged arrays

        #region Simple arrays

        public static int[] getArrOfIncrementalInts(int iArrSize, int iStartVal, int iIncrease)
        { // Gets a list of increasing int values
            int[] finalArr = new int[iArrSize];
            int iCurrVal = iStartVal;
            for (int iCells = 0; iCells < iArrSize; iCells++)
            {
                finalArr[iCells] = iCurrVal;
                iCurrVal += iIncrease;
            }
            return finalArr;
        }//getLstOfIncrementalInts

        public static int getArrPosForValueDbl(List<double> lstVals, double dVal)
        { // returns the index for the array where it contains a certain value
            for (int iVals = 0; iVals < lstVals.Count; iVals++)
                if (lstVals[iVals] == dVal)
                    return iVals;
            return -1;
        }//getArrPosForValueDbl

        public static T[] SubArray<T>(this T[] data, int index, int length)
        { // Return array from data array from index to index + length
            if (index + length > data.Length) length = data.Length - index;
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }//SubArray<T>

        #endregion Simple arrays

        #endregion Methods

    }//of class
}
