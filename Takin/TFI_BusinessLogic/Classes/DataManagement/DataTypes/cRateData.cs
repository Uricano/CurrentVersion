using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;

// Used namespaces
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.General;
using TFI.BusinessLogic.Enums;
using Cherries.TFI.BusinessLogic.Portfolio;
using Cherries.TFI.BusinessLogic.Securities.RiskFree;
using Cherries.TFI.BusinessLogic.GMath;
using Cherries.TFI.BusinessLogic.GMath.StaticMethods;
using TFI.BusinessLogic.Interfaces;
using Cherries.Models.App;
using Cherries.Models.dbo;

namespace Cherries.TFI.BusinessLogic.DataManagement.DataTypes
{
    public class cRateData : IRateData
    {

        #region Data members

        // Main variables
        private IPortfolioBL m_objPortfolio; // Current portfolio
        private ISecurity m_objRelevantSec; // Security class relevant to current prices
        private IErrorHandler m_objErrorHandler; // Error handler class

        // Data variables
        private List<Rate> m_dtMainRates = null; // Main datatable containing rate value for given security
                                                 //private List<PriceReturn> m_dtRatesBacktesting = null; //  datatable containing rates value for given security for backtesting graph
        private List<PriceReturn> m_dtPriceReturns = null; // Main datatable containing price returns for given security
        private List<PriceReturn> m_dtBacktestingReturns = null; // Datatable containing relevant price returns for backtesting
        private enumDateFreq m_enFrequency = cProperties.Frequency; // Date frequency
        private cDateRange m_drRange; // Date range of calculation data

        private double m_dFinalRate = 0D; // Final rate of security
        private double m_dClassicRate = 0D; // Classic method for calculating returns
        //private double m_dClassicRateAnnual = 0D; // The annual return for the classic return
        private double m_dRateAvg = 0D; // Average rate value
        private double m_dOneYearRateAvg = 0D; // Average rate value (for a one year period)
        private double m_dReturnWeekly = 0D;
        private Boolean m_isMinimized = false; // Checks whether the minimized returns have been calculated

        #endregion Data members

        #region Constructors, Initialization & Destructor

        public cRateData(ISecurity cSec, IPortfolioBL cPort, Boolean isFull)
        {
            m_objRelevantSec = cSec;
            m_objPortfolio = cPort;
            m_objErrorHandler = m_objPortfolio.cErrorLog;

            try
            {
                initDataVariables();
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//constructor

        #endregion Constructors, Initialization & Destructor

        #region Methods

        #region Data initialization

        private void initDataVariables()
        { // Initializes data variables - to check if recalculation is necessary
            m_enFrequency = cProperties.Frequency;
            m_drRange = new cDateRange(m_objRelevantSec.DateRange.StartDate, m_objRelevantSec.DateRange.EndDate);

            //if (m_dtMainRates != null) m_dtMainRates.Clear();
            //m_dtMainRates = new List<Rate>();
        }//initDataVariables


        #endregion Data initialization

        #region Rate calculations

        public void setUpdatedRateData(cDateRange drPeriod)
        { // Refreshes / calculates the rate data
            try
            {
                if (m_objRelevantSec.PriceTable == null)
                { m_objRelevantSec.disableCurrentSecurity(); return; }

                // Retrieves relevant price returns for backtesting
                m_dtBacktestingReturns = cBasicStaticCalcs.getPriceReturnsInDateRange(m_dtPriceReturns, drPeriod);

                // Calculates minimized weekly returns
                //if (!m_isMinimized)
                    m_dtBacktestingReturns = cBasicStaticCalcs.getMinimizedRates(m_dtBacktestingReturns, m_objRelevantSec);

                // Final return
                setNewSecFinalRate(drPeriod);

                // Irregularities handling
                if (cMath.isNaN(m_dFinalRate)) // invalid result
                { m_dFinalRate = 0D; m_objRelevantSec.disableCurrentSecurity(); return; }

                //if (cGeneralFunctions.isBelowNumberOfMonths(m_objRelevantSec.DateRange, cProperties.MinMonthsData) || (m_dFinalRate < 0D) || (m_dtBacktestingReturns.Count == 0) || (m_dtBacktestingReturns.Count < 10))
                if (cGeneralFunctions.isReturnsBelowNumberOfMonths(m_dtBacktestingReturns, cProperties.MinMonthsData) || (m_dFinalRate < 0D) || (m_dtBacktestingReturns.Count == 0) || (m_dtBacktestingReturns.Count < 10))
                { m_objRelevantSec.disableCurrentSecurity(); return; } // At least x months of data for calculation

                m_objRelevantSec.setSecurityActivity(true);
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//setUpdatedRateData

        public void setFinalReturnFromServer(cDateRange drPeriod)
        {
            //m_dtPriceReturns = cBasicStaticCalcs.getPriceReturnsInDateRange(m_dtPriceReturns, drPeriod);
            //m_dtBacktestingReturns = m_dtPriceReturns;


            //setNewSecFinalRate(drPeriod);

            m_dFinalRate = m_objRelevantSec.AvgYield * 52;
            m_dReturnWeekly = m_objRelevantSec.AvgYield;
            m_objRelevantSec.CovarClass.StandardDeviation = m_objRelevantSec.StdYield * Math.Sqrt(52);
            m_objRelevantSec.CovarClass.StDevWeekly = m_objRelevantSec.StdYield;
        }//setFinalReturnFromServer

        #endregion Rate calculations

        #region Final rate calculations

        public void setFinalReturn(double dFinalReturn)
        { // Sets final return values
            m_dFinalRate = dFinalReturn;
            m_dReturnWeekly = dFinalReturn;
            m_dRateAvg = dFinalReturn;
        }//setFinalReturn

        private void setNewSecFinalRate(cDateRange drPeriod)
        { // Sets the average values for the rate series of a given security
            if (m_dtPriceReturns.Count == 0) return;
            //double dYears = (cProperties.CalcRange.EndDate - cProperties.CalcRange.StartDate).Days / 365D;
            try
            {
                m_dRateAvg = cBasicStaticCalcs.getAvgValue(m_dtBacktestingReturns);

                m_dClassicRate = cBasicStaticCalcs.getClassicRateValue(m_objRelevantSec.PriceTable, m_objPortfolio.Details.CalcCurrency, drPeriod); // Uriel on 07/09/2016

                m_dFinalRate = cBasicStaticCalcs.getFinalReturnValue(m_dtBacktestingReturns, drPeriod);

            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
            m_dReturnWeekly = m_dFinalRate / 52;
        }//setSecRateDataAvg

        #endregion Final rate calculations

        #region Date frequency

        #endregion Date frequency

        #endregion Methods

        #region Properties

        public List<Rate> RatesData
        { get { return m_dtMainRates; } }//RatesData

        //public List<PriceReturn> RatesBacktesting
        //{ 
        //    get { return m_dtRatesBacktesting; }
        //    set { m_dtRatesBacktesting = value; }
        //}//RatesBacktesting

        public List<PriceReturn> PriceReturns
        {
            get { return m_dtPriceReturns; }
            set { m_dtPriceReturns = value; }
        }//PriceReturns

        public List<PriceReturn> BacktestingReturns
        {
            get { return m_dtBacktestingReturns; }
            set { m_dtBacktestingReturns = value; }
        }//BacktestingReturns

        public double FinalRate
        {  get { return m_dFinalRate; }  }//FinalRate

        public double ExpectedReturnAvg
        { get { return m_dRateAvg; } }//ExpectedReturnAvg

        public double RateAvgOneYear
        { get { return m_dOneYearRateAvg; } }//RateAvgOneYear

        public double WeeklyReturn
        { get { return m_dReturnWeekly; } }//WeeklyReturn

        public Boolean isMinimized
        {
            get { return m_isMinimized; }
            set { m_isMinimized = value; }
        }//isMinimized

        #endregion Properties

    }//of class
}
