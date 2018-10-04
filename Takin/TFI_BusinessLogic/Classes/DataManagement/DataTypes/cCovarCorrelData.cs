using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

// Used namespaces
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.Securities.RiskFree;
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Portfolio;
using Cherries.TFI.BusinessLogic.GMath;
using Cherries.TFI.BusinessLogic.GMath.StaticMethods;
using Cherries.TFI.BusinessLogic.Collections;
using TFI.BusinessLogic.Interfaces;
using Cherries.Models.App;

namespace Cherries.TFI.BusinessLogic.DataManagement.DataTypes
{
    public class cCovarCorrelData : ICovarCorrelData
    { // Handles Covariance and Correlation calculations

        #region Data members

        // Main variables
        private ISecurity m_objRelevantSec; // Security class relevant to current prices
        private ICollectionsHandler m_objColHandler; // Collection handler (of securities and categories)
        private IErrorHandler m_objErrorHandler; // Error handler class

        // Data variables
        private double m_dAvgVal = 0D; // Average value
        private double m_dStDevVal = 0D; // Standard deviation
        private double m_dStDevWeekly = 0D; // Standard deviation value (non-annualized)
        private double m_dSumRates = 0D; // Sum of rates 
        private double m_dVariance = 0D; // Variance of security

        // Properties variables
        private Boolean m_isCovarCalc = true; // Covariance calculation (false = correlation)

        #endregion Data members

        #region Constructors, Initialization & Destructor

        public cCovarCorrelData(ISecurity cSec, ICollectionsHandler cColHandler, IErrorHandler cErrors, Boolean isCovar)
        {
            m_objRelevantSec = cSec;
            m_objColHandler = cColHandler;
            m_objErrorHandler = cErrors;
            m_isCovarCalc = isCovar;
        }//constructor

        #endregion Constructors, Initialization & Destructor

        #region Methods

        #region Calculation handling

        public void setCalculationStatistics(Boolean isBacktest)
        { // Prepares the statistics for the calculations
            List<PriceReturn> dtMain = (isBacktest) ? m_objRelevantSec.RatesClass.BacktestingReturns : m_objRelevantSec.RatesClass.PriceReturns;

            //if (m_objRelevantSec.RatesClass.isMinimized)
                dtMain = cBasicStaticCalcs.getMinimizedRates(dtMain, m_objRelevantSec);

            m_dAvgVal = cBasicStaticCalcs.getAvgForVariance(m_objRelevantSec);
            m_dStDevVal = cBasicStaticCalcs.getNewVarianceValue(dtMain, m_dAvgVal, m_objRelevantSec);

            m_dStDevVal = Math.Sqrt(m_dStDevVal); // Variance to StDev
            m_dStDevWeekly = m_dStDevVal;
            m_dStDevVal = m_dStDevVal * Math.Sqrt(52); // Annualization
            m_dStDevVal = Math.Abs(m_dStDevVal);

            m_objRelevantSec.Analytics.RateToRiskRatio = getRateToRiskRatio();

            if (m_dStDevVal > cProperties.maxRiskVal) // Disables security if the risk is too great
                m_objRelevantSec.disableCurrentSecurity();
        }//setCalculationStatistics

        private double getRateToRiskRatio()
        { // Retrieves the rate to risk ratio 
            if (m_dStDevVal == 0D) return 0D;
            return m_objRelevantSec.RatesClass.FinalRate / (double)m_dStDevVal;
        }//getRateToRiskRatio

        #endregion Calculation handling

        #endregion Methods

        #region Properties

        public double Average
        { get { return m_dAvgVal; } }//Average

        public double SumRates
        {
            get { return m_dSumRates; }
            set { m_dSumRates = value; }
        }//SumRates

        public double Variance
        {
            get { return m_dVariance; }
            set { m_dVariance = value; }
        }//Variance

        public double StandardDeviation
        { 
            get { return m_dStDevVal; }
            set { m_dStDevVal = value; }
        }//StandardDeviation

        public double StDevWeekly
        {
            get { return m_dStDevWeekly; }
            set { m_dStDevWeekly = value; }
        }//StDevWeekly

        #endregion Properties

    }//of class
}
