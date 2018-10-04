using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.TFI.BusinessLogic.Securities.RiskFree
{
    public static class cRiskFreeRates
    {

        #region Data members

        // Main variables
        private static double m_dYearlyRate = 0.0242D; // Yearly nominal rate (based on american market)
        // Taken from: http://www.bloomberg.com/markets/rates-bonds/government-bonds/us/
        // Federal bonds rates (in Google)

        //// Calculation variables
        private static Boolean m_isMarketValChanged = false; // Whether the market's return value has been modified by the user
        private static double m_dMarketReturn = 0.056D; // Market return (user defined value)
        //private static double m_dYearlyInterest = 0.04D; // Yearly interest rate for treasury bill

        #endregion Data members

        #region Methods

        public static double getCurrentRiskFreeValue(DateTime dtCurr, DateTime dtStart, double dDailyInterest)
        { return (double)(dtCurr - dtStart).Days * dDailyInterest;  }//getCurrentRiskFreeValue

        #endregion Methods

        #region Properties

        public static double YearlyRate
        { get { return m_dYearlyRate; } }//YearlyRate

        public static double MarketReturn
        {
            get { return m_dMarketReturn; }
            set { m_dMarketReturn = value; }
        }//MarketReturn

        public static Boolean isMarketChanged
        {
            get { return m_isMarketValChanged; }
            set { m_isMarketValChanged = value; }
        }//isMarketChanged

        #endregion Properties

    }//of class
}
