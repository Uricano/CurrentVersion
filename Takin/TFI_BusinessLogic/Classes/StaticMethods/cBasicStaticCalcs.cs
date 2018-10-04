using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

// Used namespaces
using TFI.BusinessLogic.Enums;
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.Models.dbo;
using TFI.BusinessLogic.Interfaces;
using Entities = TFI.Entities;
using Cherries.Models.App;

namespace Cherries.TFI.BusinessLogic.GMath.StaticMethods
{
    static class cBasicStaticCalcs
    {

        #region Rates

        #region Price return methods

        public static void calculatePriceReturns(List<Entities.dbo.Price> dtPrices, ref List<Rate> dtOut, cDateRange cDrCalc, enumDateFreq enCalcFreq, String idSec, String strSymbol)
        { calculatePriceReturns(dtPrices, ref dtOut, cDrCalc, enCalcFreq, 0, dtPrices.Count - 1, idSec, strSymbol); }//calculatePriceReturns
        public static void calculatePriceReturns(List<Entities.dbo.Price> dtPrices, ref List<Rate> dtOut, cDateRange cDrCalc, enumDateFreq enCalcFreq, int iPosStart, int iPosEnd, String idSec, String strSymbol)
        { // Calculates price returns for the given table of prices
            DateTime dtCurrent = DateTime.Now, dtNextVal;
            int iNextIndex = 0;
            for (int iRows = iPosStart; iRows <= iPosEnd;)
            { // Goes through prices backwards
                dtCurrent = dtPrices[iRows].dDate;
                if ((dtCurrent < cDrCalc.StartDate) || (dtCurrent > cDrCalc.EndDate)) { iRows++; continue; } // No calculations if exceeds calculation range

                dtNextVal = getNextDate(dtCurrent, enCalcFreq);
                iNextIndex = getClosestDateRowIndex(dtPrices, iRows, dtNextVal, cDrCalc);

                if ((iNextIndex == -1) || (iNextIndex >= iPosEnd) || ((iPosEnd - iRows) < 7))
                    return; // Finished

                double dRateVal = getCalcRateVal(Convert.ToDouble(dtPrices[iRows].fClose), Convert.ToDouble(dtPrices[iNextIndex].fClose)); // calculated rate
                dtOut.Add(new Rate { Date = dtCurrent, RateVal = dRateVal });

                if (iNextIndex > iRows)
                    iRows = iNextIndex;
                else iRows++;
            }//main for
        }//calculatePriceReturns

        public static double getCalcRateVal(double dCurrPrice, double dPrevPrice)
        { // Calculates rate value by comparing a price to the previous price
            if ((dPrevPrice == 0D) || (dCurrPrice == 0D)) return 0D; // No given rate

            double dRateVal = System.Math.Abs(dCurrPrice / (double)dPrevPrice) - 1;
            return dRateVal;
        }//getCalcRateVal

        public static double getAdjustedPrice(double dCurrVal, double dFac)
        { // Calculates the adjusted price based on the previous price and the security's coefficient
            return dCurrVal / (double)dFac;
        }//getAdjustedPrice

        #endregion Price return methods

        #region Final rates methods

        public static double getClassicRateValue(List<Entities.dbo.Price> dtMain, string currencyType, cDateRange drPeriod)
        { // Retrieves the Final rate value for a given security

            double prevPrc, currPrc;

            List<Entities.dbo.Price> prevPrice = new List<Entities.dbo.Price>();
            List<Entities.dbo.Price> currPrice = new List<Entities.dbo.Price>();

            var nearestDiff = dtMain.Min(x => Math.Abs((x.dDate.Date - drPeriod.StartDate.Date).Ticks));
            prevPrice = dtMain.Where(x => Math.Abs((x.dDate.Date - drPeriod.StartDate.Date).Ticks) == nearestDiff).ToList(); // should find 1 row on that day

            nearestDiff = dtMain.Min(x => Math.Abs((x.dDate.Date - drPeriod.EndDate.Date).Ticks));
            currPrice = dtMain.Where(x => Math.Abs((x.dDate.Date - drPeriod.EndDate.Date).Ticks) == nearestDiff).ToList(); // should find 1 row on that day

            //prevPrice = dtMain.Where(x => x.dDate.Date == drPeriod.StartDate.Date).ToList(); // should find 1 row on that day
            //currPrice = dtMain.Where(x => x.dDate.Date == drPeriod.EndDate.Date).ToList();   // should find 1 row on that day

            if(prevPrice.Count == 0 || currPrice.Count == 0)
                return 0D;

            if (currencyType == "9999")
            {
                prevPrc = (double)prevPrice[0].fNISClose;
                currPrc = (double)currPrice[0].fNISClose;
            }
            else
            {
                prevPrc = (double)prevPrice[0].fClose;
                currPrc = (double)currPrice[0].fClose;
            }
            if (prevPrc == 0D) return 0D;
            return getCalcRateVal(currPrc, prevPrc);
        }//getClassicRateValue

        public static double getFinalReturnValue(List<PriceReturn> dtMain, cDateRange drPeriod)
        { // Returns final return value for period
            /// New Formula: multiply by specific coefficient for each year
            if ((dtMain == null) || (dtMain.Count == 0)) return 0D;

            // Calculate each year return
            List<double> yearlyReturns = getSeperateYearsReturns(dtMain, drPeriod);

            // Calculate final return
            int iNbWeeks = 0;
            double zVal = 0D;
            double dFinalVal = 0D;

            if (dtMain.Count > 105)
                // Above 2 years (CASES 1 + 2)
                dFinalVal = (1 + yearlyReturns[0] * 0.4D) * (1 + yearlyReturns[1] * 0.24D) * (1 + yearlyReturns[2] * 0.15D) - 1;
            else {
                if ((dtMain.Count == 104) || (dtMain.Count == 105)) 
                    // Exactly 2 years (CASE 3)
                    dFinalVal = (1 + yearlyReturns[0] * 0.53D) * (1 + yearlyReturns[1] * 0.31D) - 1;
                else {
                    if (dtMain.Count > 52)
                    { // Between 1 & 2 years (CASE 4)
                        iNbWeeks = dtMain.Count - 52;
                        zVal = (double)(104 - iNbWeeks) / 52D;

                        dFinalVal = (1 + yearlyReturns[0] * 0.53D * zVal) * (1 + yearlyReturns[1] * 0.31D * ((double)iNbWeeks / 52D)) - 1;
                    } else {
                        if (dtMain.Count == 52)
                            // Exactly 1 year (CASE 5)
                            dFinalVal = yearlyReturns[0];
                        else {
                            if (dtMain.Count < 52)
                                // Below 1 year (CASE 6)
                                dFinalVal = yearlyReturns[0] * (52D / (double)dtMain.Count);
                        }
                    }
                }
            }

            return dFinalVal;
        }//getFinalReturnValue

        private static List<double> getSeperateYearsReturns(List<PriceReturn> dtMain, cDateRange drPeriod)
        { // Calculates final return for each seperate year in collection
            double T1 = 0D, T2 = 0D, T3 = 0D;
            List<PriceReturn> prStart = new List<PriceReturn>(), prEnd = new List<PriceReturn>();
            List<double> colReturns = new List<double>();

            // T1
            prEnd.Add(dtMain[dtMain.Count - 1]);

            if (dtMain.Count <= 52) // Below a year
                prStart.Add(dtMain[0]);
            else prStart.Add(dtMain[dtMain.Count - 53]);

            //var nearestDiff = dtMain.Min(x => Math.Abs((x.dtDate.Date - prEnd[0].dtDate.AddYears(-1)).Ticks));
            //prStart = dtMain.Where(x => Math.Abs((x.dtDate.Date - prEnd[0].dtDate.AddYears(-1)).Ticks) == nearestDiff).ToList(); // should find 1 row on that day

            T1 = ((double)prEnd[0].fAdjClose / (double)prStart[0].fAdjClosePrevWeek) - 1D;
            colReturns.Add(T1);

            // T2
            if (dtMain.Count > 53)
            { // Second Year
                prEnd.Clear();
                prEnd.Add(dtMain[dtMain.Count - 54]);

                prStart.Clear();
                if (dtMain.Count <= 105) // Below 2 years
                    prStart.Add(dtMain[0]);
                else prStart.Add(dtMain[dtMain.Count - 106]);


                //nearestDiff = dtMain.Min(x => Math.Abs((x.dtDate.Date - prEnd[0].dtDate.AddYears(-1)).Ticks));
                //prStart = dtMain.Where(x => Math.Abs((x.dtDate.Date - prEnd[0].dtDate.AddYears(-1)).Ticks) == nearestDiff).ToList(); // should find 1 row on that day

                if (prStart.Count > 0)
                    T2 = ((double)prEnd[0].fAdjClose / (double)prStart[0].fAdjClosePrevWeek) - 1D;
                colReturns.Add(T2);
            } else colReturns.Add(0D);

            // T3
            if (dtMain.Count > 105)
            { // If a second year exists in price returns collection
                prEnd.Clear();
                prEnd.Add(dtMain[dtMain.Count - 106]);

                prStart.Clear();
                if (dtMain.Count < 156) // Below 2 years
                    prStart.Add(dtMain[0]);
                else prStart.Add(dtMain[dtMain.Count - 156]);

                //nearestDiff = dtMain.Min(x => Math.Abs((x.dtDate.Date - prEnd[0].dtDate.AddYears(-1)).Ticks));
                //prStart = dtMain.Where(x => Math.Abs((x.dtDate.Date - prEnd[0].dtDate.AddYears(-1)).Ticks) == nearestDiff).ToList(); // should find 1 row on that day

                if (prStart.Count > 0)
                    T3 = ((double)prEnd[0].fAdjClose / (double)prStart[0].fAdjClosePrevWeek) - 1D;
                colReturns.Add(T3);
            } else colReturns.Add(0D);

            return colReturns;
        }//getSeperateYearsReturns

        public static List<PriceReturn> getPriceReturnsInDateRange(List<PriceReturn> colReturns, cDateRange drPeriod)
        { // Returns a collection of price returns relevant to date range
            List<PriceReturn> colFinal = new List<PriceReturn>();
            for (int iRows = 0; iRows < colReturns.Count; iRows++)
                if ((colReturns[iRows].dtDate >= drPeriod.StartDate) && (colReturns[iRows].dtDate <= drPeriod.EndDate))
                    colFinal.Add(colReturns[iRows]);

            return colFinal;
        }//getPriceReturnsInDateRange

        public static double getCummulativeReturnValue(DataTable dtMain)
        { return getCummulativeReturnValue(dtMain, 0, dtMain.Rows.Count - 1); }//getCummulativeReturnValue
        public static double getCummulativeReturnValue(DataTable dtMain, int iStartPos, int iEndPos)
        { // Calculates the cummulative return of current security
            if (dtMain.Rows.Count < iEndPos) return 0D;
            double dFinalVal = 0D;
            for (int iRows = iStartPos; iRows <= iEndPos; iRows++) // Goes through price returns
                dFinalVal += Convert.ToDouble(dtMain.Rows[iRows]["dRateVal"]);

            return dFinalVal;
        }//getCummulativeReturnValue

        #endregion Final rates methods

        #region Helping methods

        private static int getClosestDateRowIndex(List<Entities.dbo.Price> dtMain, int iPrevIndex, DateTime dtCompare, cDateRange cDrCalc)
        {//get the most nearly index to request date return -1 if no more dates in the table
            int iFinalRow = iPrevIndex, iMinDiff = int.MaxValue;
            TimeSpan tsDateDiff;
            DateTime dtTmp;
            for (int iRows = iFinalRow + 1; iRows < dtMain.Count; iRows++)
            { // Finds closest date row index
                dtTmp = dtMain[iRows].dDate;
                if (dtTmp >= cDrCalc.StartDate && dtTmp <= cDrCalc.EndDate)
                {
                    tsDateDiff = dtCompare.Subtract(dtTmp);
                    if (System.Math.Abs(tsDateDiff.Days) <= iMinDiff) { iFinalRow = iRows; iMinDiff = System.Math.Abs(tsDateDiff.Days); } // Marks smaller date differences
                    else return iFinalRow; // Once difference is bigger - stop (we deal with a sorted table)
                }
            }
            if (iFinalRow == iPrevIndex) return -1;
            return iFinalRow;
        }//getClosestDateRowIndex

        private static DateTime getNextDate(DateTime dtCurr, enumDateFreq enCalcFreq)
        { // retrieves the next date available based on the selected frequency
            TimeSpan tsPeriod = new TimeSpan(4, 12, 0, 0) + new TimeSpan(2, 12, 0, 0); // one week
            DateTime dtFinal = dtCurr;
            switch (enCalcFreq)
            {
                case enumDateFreq.Daily:
                    return dtFinal.AddDays(-1);
                case enumDateFreq.Weekly:
                    return dtFinal - tsPeriod;
            }
            return dtFinal;
        }//getNextDate

        

        #endregion Helping methods

        #endregion Rates

        #region Covariance / Correlation

        public static double getNewCovarianceValue(List<PriceReturn> dtTbl1, double dAvgVal1, List<PriceReturn> dtTbl2, double dAvgVal2)
        { // Calculates covariance value between 2 Datatables
            var query = from sec1 in dtTbl1
                        join sec2 in dtTbl2
                             on sec1.dtDate equals sec2.dtDate
                        select new
                        {
                            sec1.dtDate,
                            Return1 = sec1.dMiniReturn,
                            Return2 = sec2.dMiniReturn
                        };

            double dFinalVal = 0D; int iCounter = 0;
            foreach (var item in query)
            { // Goes through all items in query
                dFinalVal += ((Convert.ToDouble(item.Return1) - dAvgVal1) * (Convert.ToDouble(item.Return2) - dAvgVal2));
                iCounter++;
            }

            if ((iCounter == 1) || (dFinalVal == 0D)) return 0D;
            //return dFinalVal / ((double)iCounter - 1);
            return dFinalVal / ((double)51);
        }//getCovarianceValue

        public static double getCorrelValue(DataTable dtTbl1, double dAvgVal1, double dStDev1, DataTable dtTbl2, double dAvgVal2, double dStDev2, String strValFld)
        { // Calculates correlation value
            double dMultAvg = getFldMultiplicationAvg(dtTbl1, strValFld, dtTbl2, strValFld);
            double dNominator = dMultAvg - (dAvgVal1 * dAvgVal2);
            double dDenominator = (dStDev1 * dStDev2); 

            if (dDenominator == 0D) return 0D;
            return dNominator / (double)dDenominator;
        }//getCorrelValue

        #endregion Covariance / Correlation

        #region Basic math methods

        public static List<PriceReturn> getMinimizedRates(List<PriceReturn> dtMain, ISecurity cSec)
        { // Calculates minimized rates for given security
            if ((dtMain == null) || (dtMain.Count == 0)) return dtMain;

            if (dtMain.Count > 105)
                for (int iRows = dtMain.Count - 1; iRows >= 0; iRows--) 
                { // Above 2 years
                    if (iRows >= (dtMain.Count - 52)) 
                    { dtMain[iRows].dMiniReturn = dtMain[iRows].dReturn * 0.4D; continue; }
                    if ((iRows < (dtMain.Count - 52)) && (iRows >= (dtMain.Count - 104)))
                    { dtMain[iRows].dMiniReturn = dtMain[iRows].dReturn * 0.24D; continue; }

                    dtMain[iRows].dMiniReturn = dtMain[iRows].dReturn * 0.15D; continue;
                }
            if ((dtMain.Count > 52) && (dtMain.Count <= 105))
                for (int iRows = dtMain.Count - 1; iRows >= 0; iRows--)
                { // Above 1 year
                    if (iRows >= (dtMain.Count - 52))
                    { dtMain[iRows].dMiniReturn = dtMain[iRows].dReturn * 0.53D; continue; }

                    dtMain[iRows].dMiniReturn = dtMain[iRows].dReturn * 0.31D;
                }
            if (dtMain.Count <= 52)
                for (int iRows = 0; iRows < dtMain.Count; iRows++)
                    dtMain[iRows].dMiniReturn = dtMain[iRows].dReturn;

            cSec.RatesClass.isMinimized = true;
            return dtMain;
        }//setMinimizedRates

        public static double getAvgForVariance(ISecurity cCurrSec)
        { // Calculates average value for variance calculation
            return cCurrSec.RatesClass.WeeklyReturn;
        }//getAvgForVariance

        public static double getAvgValue(List<PriceReturn> dtMain)
        { // Calculates average value for a given datatble
            if (dtMain.Count == 0) return 0D;
            double dFinalVal = 0D;
            for (int iRows = 0; iRows < dtMain.Count; iRows++)
                dFinalVal += Convert.ToDouble(dtMain[iRows].dReturn);

            dFinalVal = dFinalVal / (double)(dtMain.Count - 1);
            return dFinalVal;
        }//getAvgValue

        public static double getNewVarianceValue(List<PriceReturn> dtMain, double dAvgVal, ISecurity cCurrSec)
        { // Calculates the variance for a given Datatable
            if (dtMain.Count <= 1) return 0D;
            double dFinalVal = 0D;
            for (int iRows = 0; iRows < dtMain.Count; iRows++)
                dFinalVal += System.Math.Pow(Convert.ToDouble(dtMain[iRows].dMiniReturn) - dAvgVal, 2);

            cCurrSec.CovarClass.SumRates = dFinalVal;
            dFinalVal = (dFinalVal / (double)(dtMain.Count - 1)); // Variance
            cCurrSec.CovarClass.Variance = dFinalVal;
            return dFinalVal;
        }//getNewVarianceValue

        public static double getVarianceValue(List<PriceReturn> dtMain, double dAvgVal, ISecurity cCurrSec)
        { // Calculates the variance for a given Datatable
            if (dtMain.Count <= 1) return 0D;
            double dFinalVal = 0D;
            for (int iRows = 0; iRows < dtMain.Count; iRows++)
                dFinalVal += System.Math.Pow(Convert.ToDouble(dtMain[iRows].dReturn) - dAvgVal, 2);
            
            cCurrSec.CovarClass.SumRates = dFinalVal;
            dFinalVal = (dFinalVal / (double)(dtMain.Count - 1)); // Variance
            cCurrSec.CovarClass.Variance = dFinalVal;
            return dFinalVal;
        }//getVarianceValue

        private static double getFldMultiplicationAvg(DataTable dtFld1, String sFldVal1, DataTable dtFld2, String sFldVal2)
        { // Returns average of field multiplication, out of 2 seperate fields in 2 seperate Datatables 
            double dFinalVal = 0D;
            if (!(dtFld1.Rows.Count == dtFld2.Rows.Count)) return 0;

            for (int iRows = 0; iRows < dtFld1.Rows.Count; iRows++)
                dFinalVal += Convert.ToDouble(dtFld1.Rows[iRows][sFldVal1]) *
                    Convert.ToDouble(dtFld2.Rows[iRows][sFldVal2]);
            return dFinalVal / (double)dtFld1.Rows.Count;
        }//getFldMultiplicationAvg

        #endregion Basic math methods

    }//of class
}
