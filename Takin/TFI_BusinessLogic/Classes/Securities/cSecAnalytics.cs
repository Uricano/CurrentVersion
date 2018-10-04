using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Used namespaces
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Securities.RiskFree;
using Cherries.TFI.BusinessLogic.Collections;
using TFI.BusinessLogic.Interfaces;

namespace Cherries.TFI.BusinessLogic.Securities
{
    public class cSecAnalytics
    {

        #region Data members

        // Main variables
        private ISecurity m_objRelevantSec; // Security class relevant to current prices
        private ICollectionsHandler m_objColHandler; // Collections handler
        private IErrorHandler m_objErrorHandler; // Error handler class

        // Statistics variables
        private double m_dRateToRiskRatio = 0D; // Rate to risk ratio 
        private double m_dSecBeta = 0D; // Security's beta
        private double m_dSecExpectedReturn = 0D; // Security's Expected return (by CAPM Calculation)
        private double m_dRelativeSharpe = 0D; // Relative Sharpe value
        private double m_dSortino = 0D; // Sortino final value
        private double m_dRomadVal = 0D; // ROMAD value for security
        private double m_dTreynorValue = 0D; // Treynor for current security
        private double m_dShortfallRisk = 0D; // Shortfall Risk value

        // Data variables
        private double m_dCurrWeight = 0D; // Current security weight (on Ef position)
        private double m_dCurrQuantity = 0D; // Current security quantity (on Ef position)
        private double m_dLastPrice = 0D; // Last price of security (for portfolio)

        // Calculation variables
        private DateTime m_dtStopDate; // Stop date for one year backwards data
        private List<double> m_colSecTbDifferences = new List<double>(); // Collection of the differences between security rates and TB rates
        private double m_dPeriodInterest = 0D; // Interest for entirety of period (based on yearly duration)
        private double m_dDailyInterest = 0D; // Daily interest (for risk-free calculation)
        private double m_dTBAverage = 0D; // Average of TB values
        private double m_dSortinoDesiredRate = 0D; // Desired rate value (for Sortino calculation)
        
        #endregion Data members

        #region Constructors, Initialization & Destructor

        public cSecAnalytics(ISecurity cCurrsec, IErrorHandler cErrors, ICollectionsHandler cColHandler)
        {
            m_objErrorHandler = cErrors;
            m_objColHandler = cColHandler;
            m_objRelevantSec = cCurrsec;
        }//constructor

        #endregion Constructors, Initialization & Destructor

        #region Methods

        #region General methods

        public void calculateSecurityAnalytics()
        { // Calculates all of the analytics values for the given security
            try
            {
                //setExpectedReturnCAPM();
                setSortinoValue();
                //setROMAdValue();
                setTreynorValue();
                setShortfallRiskVal();
                setRelativeSharpeValue();
                //if (!isResultsValid()) m_objRelevantSec.disableCurrentSecurity();
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//calculateSecurityAnalytics
        
        private Boolean isResultsValid()
        { // Checks whether the received results are acceptable (if not - disable security)
            if ((m_dRelativeSharpe > 6D) || (m_dRelativeSharpe < -3D)) return false;
            if ((m_dSecBeta < -1D) || (m_dSecBeta > 10D)) return false;
            return true;
        }//isResultsValid

        #endregion General methods

        #region Expected return
        
        private double getRiskFreeVariance()
        { // calculates the variance of the risk free asset
            double dFinalVal = 0D; double dCurrRiskFreeVal; int iCount = 0;
            for (int iRows = 0; iRows < m_objRelevantSec.RatesTable.Count; iRows++, iCount++)
            { // Goes through security Data
                dCurrRiskFreeVal = cRiskFreeRates.getCurrentRiskFreeValue(Convert.ToDateTime(m_objRelevantSec.RatesTable[iRows].Date),
                        m_dtStopDate, m_objRelevantSec.Analytics.DailyInterest);

                dFinalVal += Math.Pow(dCurrRiskFreeVal - m_dTBAverage, 2);

                if (Convert.ToDateTime(m_objRelevantSec.RatesTable[iRows].Date) <= m_objRelevantSec.Analytics.OneYearDate) break;
            }
            return dFinalVal / (double)iCount;
        }//getRiskFreeVariance

        #endregion Expected return

        #region Sharpe Ratios

        private void setRelativeSharpeValue()
        { // Calculates the relative Sharpe value
            try
            {
                if (m_objRelevantSec.CovarClass.StandardDeviation == 0D) { m_dRelativeSharpe = 0D; return; }

                m_dRelativeSharpe = (m_objRelevantSec.RatesClass.RateAvgOneYear - cRiskFreeRates.YearlyRate) / (double)getStDevDifferences();
                //m_dRelativeSharpe = (m_objRelevantSec.RatesClass.ClassicRate - cRiskFreeRates.YearlyRate) / (double)m_objRelevantSec.CovarClass.StandardDeviation;

            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//setRelativeSharpeValue

        private double getStDevDifferences()
        { // Returns the standard deviation for the rate differences (between security and TB rates)
            List<double> colDiffs = getListofDifferencesFromRiskFree(false);
            double dDiffAvg = 0D;
            for (int iDiffs = 0; iDiffs < colDiffs.Count; iDiffs++)
                dDiffAvg += colDiffs[iDiffs];
            dDiffAvg = dDiffAvg / (double)colDiffs.Count; // Average value

            double dStDev = 0D;
            for (int iDiffs = 0; iDiffs < colDiffs.Count; iDiffs++)
                dStDev += Math.Pow(colDiffs[iDiffs] - dDiffAvg, 2);
            dStDev = dStDev / (double)colDiffs.Count;
            return Math.Sqrt(dStDev);
        }//getStDevDifferences

        #endregion Sharpe Ratios

        #region Sortino

        private void setSortinoValue()
        { // Calculates the values of Sortino for the current security
            try
            {
                if (m_dSortinoDesiredRate == 0D) m_dSortinoDesiredRate = cRiskFreeRates.YearlyRate;
                m_colSecTbDifferences = getListofDifferencesFromRiskFree(true);
                double dDownside = getDownsideVolatility(m_colSecTbDifferences);

                m_dSortino = (m_objRelevantSec.RatesClass.FinalRate - cRiskFreeRates.YearlyRate) / (double)dDownside;
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//setSortinoValue

        private double getDownsideVolatility(List<double> colDiffs)
        { // Returns the downside volatility needed to calculate Sortino
            double dDownside = 0D;
            for (int iDiffs = 0; iDiffs < colDiffs.Count; iDiffs++)
                dDownside += Math.Pow(colDiffs[iDiffs], 2);
            dDownside = Math.Sqrt(dDownside / (double)colDiffs.Count);
            return dDownside;
        }//getDownsideVolatility

        private List<double> getListofDifferencesFromRiskFree(Boolean isNegative)
        { // Retrieves the list of different values from those of the risk free
            // Param isNegative: whether to create a list of only the negative differences
            List<double> colDifferences = new List<double>();
            double dDiff = 0D; double dCurrRiskFreeVal;
            for (int iRows = 0; iRows < m_objRelevantSec.RatesTable.Count; iRows++)
            { // Goes through security rates
                dCurrRiskFreeVal = cRiskFreeRates.getCurrentRiskFreeValue(Convert.ToDateTime(m_objRelevantSec.RatesTable[iRows].Date),
                        m_dtStopDate, m_objRelevantSec.Analytics.DailyInterest);

                dDiff = Convert.ToDouble(m_objRelevantSec.RatesTable[iRows].RateVal) - dCurrRiskFreeVal;
                if ((isNegative && (dDiff < 0D)) || !isNegative) colDifferences.Add(dDiff);

                if (Convert.ToDateTime(m_objRelevantSec.RatesTable[iRows].Date) <= m_dtStopDate) break; // Breaks if reaches one year backwards
            }
            return colDifferences;
        }//getListofDifferencesFromRiskFree

        #endregion Sortino

        #region ROMAD

        //private void setROMAdValue()
        //{ // Calculates the ROMAD value for the security
        //    try
        //    {
        //        double dMin = double.MaxValue, dMax = double.MinValue;
        //        setMinMaxPrices(ref dMin, ref dMax);
        //        m_dRomadVal = (dMax - dMin) / (double)dMax;
        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex);
        //    }
        //}//setROMAdValue

        //private void setMinMaxPrices(ref double dMinPrice, ref double dMaxPrice)
        //{ // Retrieves the minimum and maximum price values of the security (save the last val)
        //    for (int iRows = 0; iRows < m_objRelevantSec.PriceTable.Count - 1; iRows++)
        //    { // Goes through sec prices (except last one)
        //        var price = (m_ .CalcCurrency == "ILS") ? Convert.ToDouble(cSecsCol[iSecs].PriceTable[cSecsCol[iSecs].PriceTable.Count - 1].fNISClose) : Convert.ToDouble(cSecsCol[iSecs].PriceTable[cSecsCol[iSecs].PriceTable.Count - 1].fClose);
        //        if (dMinPrice > Convert.ToDouble(m_objRelevantSec.PriceTable[iRows].dAdjPrice)) dMinPrice = Convert.ToDouble(m_objRelevantSec.PriceTable[iRows].dAdjPrice);
        //        if (dMaxPrice < Convert.ToDouble(m_objRelevantSec.PriceTable[iRows].dAdjPrice)) dMaxPrice = Convert.ToDouble(m_objRelevantSec.PriceTable[iRows].dAdjPrice);

        //        if (m_objRelevantSec.PriceTable[iRows].dDate <= m_dtStopDate) break;
        //    }
        //}//setMinMaxPrices

        #endregion ROMAD

        #region Treynor

        public void setTreynorValue()
        { // Calculates the Treynor value
            if (m_dSecBeta == 0D) return; // No Zero division
            try
            {
                m_dTreynorValue = (m_objRelevantSec.RatesClass.RateAvgOneYear - cRiskFreeRates.YearlyRate) / (double)m_dSecBeta;
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//setTreynorValue

        #endregion Treynor

        #region Shortfall risk

        private void setShortfallRiskVal()
        { // Calculates the security's shortfall risk value
            try
            {
                //m_dShortfallRisk = (m_objRelevantSec.RatesClass.ExpectedReturnAvg - m_dSortinoDesiredRate) / (double)m_objRelevantSec.CovarClass.StandardDeviation;
                m_dShortfallRisk = m_objRelevantSec.RatesClass.RateAvgOneYear - (2 * m_objRelevantSec.CovarClass.StandardDeviation);
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//setShortfallRiskVal

        #endregion Shortfall risk

        #endregion Methods

        #region Properties

        #region Statistics variables

        public double RelativeSharpe
        { get { return m_dRelativeSharpe; } }//RelativeSharpe

        public double Sortino
        { get { return m_dSortino; } }//Sortino

        public double ROMAD
        { get { return m_dRomadVal; } }//ROMAD

        public double Treynor
        { get { return m_dTreynorValue; } }//Treynor

        public double ShortfallRisk
        { get { return m_dShortfallRisk; } }//ShortfallRisk

        public double RateToRiskRatio
        {
            get { return m_dRateToRiskRatio; }
            set { m_dRateToRiskRatio = value; }
        }//RateToRiskRatio

        public double Weight
        {
            get { return m_dCurrWeight; }
            set { m_dCurrWeight = value; }
        }//Weight

        public double Quantity
        {
            get { return m_dCurrQuantity; }
            set { m_dCurrQuantity = value; }
        }//Quantity

        public double LastPrice
        {
            get { return m_dLastPrice; }
            set { m_dLastPrice = value; }
        }//LastPrice

        public double CapmBeta
        {
            get { return m_dSecBeta; }
            set { m_dSecBeta = value; }
        }//CapmBeta

        public double CapmExpectedReturn
        {
            get { return m_dSecExpectedReturn; }
            set { m_dSecExpectedReturn = value; }
        }//CapmExpectedReturn

        #endregion Statistics variables

        #region Calculation variables

        public DateTime OneYearDate
        {
            get { return m_dtStopDate; }
            set { m_dtStopDate = value; }
        }//OneYearDate

        public double PeriodInterest
        {
            get { return m_dPeriodInterest; }
            set { m_dPeriodInterest = value; }
        }//PeriodInterest

        public double DailyInterest
        {
            get { return m_dDailyInterest; }
            set { m_dDailyInterest = value; }
        }//DailyInterest

        public double TbAverage
        { set { m_dTBAverage = value; } }//TbAverage

        #endregion Calculation variables

        #endregion Properties

    }//of class
}
