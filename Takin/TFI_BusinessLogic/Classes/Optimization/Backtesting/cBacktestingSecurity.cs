using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

// Used namespaces
using TFI.BusinessLogic.Interfaces;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using TFI.BusinessLogic.Enums;

namespace TFI.BusinessLogic.Classes.Optimization.Backtesting
{
    public class cBacktestingSecurity : IBacktestingSecurity
    {

        #region Data members

        // Project variables
        private IPortfolioBL m_objPortfolio; // Current Portfolio class
        private ISecurity m_objSecurity; // Current cSecurity pointer
      
        private ICollectionsHandler m_objColHandler; // Collection handler
        private IOptimizationResults m_objEfHandler; // Markowitz calculation handler
        private IErrorHandler m_objErrorHandler; // Error handler

        // Calculation parameters
        private double m_dWeight = 0D; // Security's weight
        private cDateRange m_drDateRange = new cDateRange(DateTime.Today, DateTime.Today); // Date range for backtesting
        private enumBacktestingSecTypes m_objSecType = enumBacktestingSecTypes.Stock; // Security Type

        // Data variables
        private DataTable m_dtPriceReturns; // Datatable containing all price returns for all securities
        private double m_dCurrValue = 0D; // Current monetary value of security
        private double m_dOrigValue = 0D; // Original monetary value of security

        private Boolean m_isDisabled = false; // Whether the system has disabled the current security

        #endregion Data members

        #region Consturctors, Initialization & Destructor

        public cBacktestingSecurity(IPortfolioBL cPort, ISecurity cSec, cDateRange Dates)
        {
            m_objPortfolio = cPort;
            m_objErrorHandler = m_objPortfolio.cErrorLog;
            m_objColHandler = m_objPortfolio.ColHandler;
            m_objSecurity = cSec;
            m_drDateRange = Dates;

            try
            {


            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//cSecurity constructor

        #endregion Consturctors, Initialization & Destructor

        #region Methods

        #region Base methods

        void IBacktestingSecurity.Init(IPortfolioBL cPort, ISecurity cSec, cDateRange Dates)
        {
            m_objPortfolio = cPort;
            m_objErrorHandler = m_objPortfolio.cErrorLog;

            m_objColHandler = m_objPortfolio.ColHandler;
            m_objSecurity = cSec;
            m_drDateRange = Dates;

            try
            {


            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//IBacktestingSecurity.Init

        void IBacktestingSecurity.disableCurrentSecurity(cBacktestingSecurities collection)
        { // Makes the current security non-active (can be used for various reasons)
            try
            {
                m_isDisabled = true;
                collection.Add(this);
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//IBacktestingSecurity.disableCurrentSecurity



        #endregion Base methods

        #endregion Methods

        #region Properties

        public IPortfolioBL Portfolio
        {
            get { return m_objPortfolio; }
            set { m_objPortfolio = value; }
        }//Portfolio

        public ISecurity Security
        {
            get { return m_objSecurity; }
            set { m_objSecurity = value; }
        }//Security

        public ICollectionsHandler ColHandler
        {
            get { return m_objColHandler; }
            set { m_objColHandler = value; }
        }//ColHandler

        public IOptimizationResults EfHandler
        {
            get { return m_objEfHandler; }
            set { m_objEfHandler = value; }
        }//EfHandler

        public IErrorHandler ErrorHandler
        {
            get { return m_objErrorHandler; }
            set { m_objErrorHandler = value; }
        }//ErrorHandler

        public double Weight
        {
            get { return m_dWeight; }
            set { m_dWeight = value; }
        }//Weight

        public cDateRange DateRange
        {
            get { return m_drDateRange; }
            set { m_drDateRange = value; }
        }//DateRange

        public enumBacktestingSecTypes SecType
        {
            get { return m_objSecType; }
            set { m_objSecType = value; }
        }//SecType

        public DataTable PriceReturns
        {
            get { return m_dtPriceReturns; }
            set { m_dtPriceReturns = value; }
        }//PriceReturns

        public double CurrValue
        {
            get { return m_dCurrValue; }
            set { m_dCurrValue = value; }
        }//CurrValue

        public double OrigValue
        {
            get { return m_dOrigValue; }
            set { m_dOrigValue = value; }
        }//OrigValue

        public Boolean isDisabled
        {
            get { return m_isDisabled; }
            set { m_isDisabled = value; }
        }//isDisabled


        #endregion Properties

    }//of Class
}
