using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

//Used namespaces
using Cherries.TFI.BusinessLogic.Collections;
using Cherries.TFI.BusinessLogic.DataManagement;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.DataManagement.Prices;
using Cherries.TFI.BusinessLogic.DataManagement.ImportPorts;
using Cherries.TFI.BusinessLogic.Portfolio;
using Cherries.TFI.BusinessLogic.General;
using Entities = TFI.Entities;
using Cherries.Models.dbo;
using TFI.BusinessLogic.Interfaces;
using Cherries.Models.App;
using TFI.BusinessLogic.Bootstraper;
using Ness.DataAccess.Repository;
using NHibernate.Linq;

namespace Cherries.TFI.BusinessLogic.Securities
{
    public class cSecurity : ISecurity
    {

        #region Data members

        // Main variables
        private IPortfolioBL m_objPortfolio; // Portfolio class pointer
        private ICollectionsHandler m_objColHandler; // Collection handler (of securities and categories)
        private cSecProperties m_objSecProperties; // Properties variables class
        private cSecAnalytics m_objSecAnalytics; // Analytics variables class
        private IErrorHandler m_objErrorHandler; // Error handler class

        // Data variables
        private cDateRange m_sPricesRange; // Date range of prices contained within DB
        private cPriceData m_objPrices; // Prices data for current security
        private IRateData m_objRates; // Rates data of current security
        private ICovarCorrelData m_objCovarData; // Covariance data handler
        private cCovarCorrelData m_objCorrelData; // Correlation data handler
        private double m_dFAC = 1D; // Adjusted factor for security
        private string m_idCurrency = "";  //security currency ID
        private double m_dAvgYield = 0D; // Average yield value for security
        private double m_dStdYield = 0D; // Standard deviation value for security
        private double m_dAvgYieldNIS = 0D; // Average yield value for security NIS
        private double m_dStdYieldNIS = 0D; // Standard deviation value for security NIS
        private double m_dValueUSA = 0D;       // Security Value USA
        private double m_dValueNIS = 0D;       // Security Value NIS
        private double m_dWeightUSA = 0D;       // Security Weight USA
        private double m_dWeightNIS = 0D;       // Security Weight NIS

        // General variables
        private Boolean m_isActive = true; // Security is active in current portfolio
        private Boolean m_isDisabled = false; // Whether the system has disabled the current security
        //private Boolean m_isValid = false; // Whether the security is valid for optimization
        private int m_iRecentVolume; // Latest traded volume index
        private List<double> m_colVolumes = new List<double>(); // Collection of traded volumes for each day in full date-range

        #endregion Data members

        #region Consturctors, Initialization & Destructor

        public cSecurity(IPortfolioBL cPort, string secName, string secSymbol)
        {
            m_objPortfolio = cPort;
            m_objErrorHandler = m_objPortfolio.cErrorLog;
            
            m_objColHandler = m_objPortfolio.ColHandler;
            m_objSecProperties = new cSecProperties(m_objErrorHandler, m_objPortfolio);
            m_objSecAnalytics = new cSecAnalytics(this, m_objErrorHandler, m_objColHandler);
            m_objSecProperties.SecurityName = m_objSecProperties.getSecName(secName); // No ' signs + trim spaces
            m_objSecProperties.SecuritySymbol = secSymbol;
            m_objSecProperties.SecColor = System.Drawing.Color.FromArgb(cProperties.RndGenerator.Next(255), cProperties.RndGenerator.Next(255), cProperties.RndGenerator.Next(255));
            m_objPrices = new cPriceData(this, m_objPortfolio);
            try
            {
                m_sPricesRange = new cDateRange(DateTime.Today.AddYears(-cProperties.DatesInterval).AddDays(-1), DateTime.Today.AddDays(-1));


                m_objRates = new cRateData(this, m_objPortfolio, false);
                m_objCovarData = new cCovarCorrelData(this, m_objColHandler, m_objErrorHandler, true);
                m_objCorrelData = new cCovarCorrelData(this, m_objColHandler, m_objErrorHandler, false);
            }
            catch (Exception ex) { m_objErrorHandler.LogInfo(ex); }
        }//cSecurity constructor

        #endregion Consturctors, Initialization & Destructor

        #region Methods

        #region Base methods

        public void Init(IPortfolioBL cPort)
        {
            m_objPortfolio = cPort;
            m_objErrorHandler = m_objPortfolio.cErrorLog;

            m_objColHandler = m_objPortfolio.ColHandler;
            m_objSecAnalytics = new cSecAnalytics(this, m_objErrorHandler, m_objColHandler);
            m_objPrices.Portfolio = cPort;
            try
            {
                //LR: why do we have 5 years of date range here ???
                m_sPricesRange = new cDateRange(DateTime.Today.AddYears(-cProperties.DatesInterval).AddDays(-1), DateTime.Today.AddDays(-1));


                m_objRates = new cRateData(this, m_objPortfolio, false);
                m_objCovarData = new cCovarCorrelData(this, m_objColHandler, m_objErrorHandler, true);
                m_objCorrelData = new cCovarCorrelData(this, m_objColHandler, m_objErrorHandler, false);
            }
            catch (Exception ex) { m_objErrorHandler.LogInfo(ex); }
        }

        public void disableCurrentSecurity()
        { // Makes the current security non-active (can be used for various reasons)
            try
            {
                m_isActive = false;
                m_isDisabled = true;
                m_objColHandler.DisabledSecs.Add(this);
                //m_objColHandler.ActiveSecs = m_objColHandler.Securities.getListOfActiveSecs();
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//disableCurrentSecurity

        public void enableCurrentSecurity()
        { // Makes the current security active
            try
            {
                m_isActive = true;
                m_isDisabled = false;
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//enableCurrentSecurity

        public void clearOptimizationData()
        { // Clears the data that was calculated in previous optimization
            m_objSecAnalytics.Weight = 0D;
            m_objSecAnalytics.Quantity = 0D;
        }//clearOptimizationData

        public void setSecurityActivity(Boolean isActive)
        { // Sets the security's desired activity level
            m_isActive = isActive;
            if (isActive) m_isDisabled = false;
        }//setSecurityActivity

        //public void setSecurityValidity()
        //{ // Verifies the security is valid for optimization (if not - it shall not be displayed)

        //    // TODO: Do we have to add check for m_dStdYieldNIS  and m_dAvgYieldNIS with  ||  (OR) in the next 2 lines????????????

        //    if (m_dAvgYield < 0D) { m_isValid = false; return; }
        //    if (m_dStdYield >= cProperties.maxRiskVal) { m_isValid = false; return; }

        //    if (m_sPricesRange != null) 
        //        if (cGeneralFunctions.isBelowNumberOfMonths(m_sPricesRange, cProperties.MinMonthsData)) { m_isValid = false; return; }
        //    m_isValid = true;
            
        //}//isValidSecurity

        #endregion Base methods

        #endregion Methods

        #region Properties

        public cSecProperties Properties
        {
            get { return m_objSecProperties; }
            set { m_objSecProperties = value; }
        }//Properties

        public cSecAnalytics Analytics
        {
            get { return m_objSecAnalytics; }
            set { m_objSecAnalytics = value; }
        }//Analytics

        public cDateRange DateRange
        {
            get { return m_sPricesRange; }
            set { m_sPricesRange = value; }
        }//DateRange

        public cPriceData PricesClass
        {
            get { return m_objPrices; }
            set { m_objPrices = value; }
        }//Prices
        public List<Entities.dbo.Price> PriceTableCurrenct
        {
            get { return m_objPrices.MainData; }
        }
        public List<Entities.dbo.Price> PriceTable
        {
            get {
                //if (m_objPrices.MainData == null)
                //{
                //    var index = StaticData<cSecurity, ISecurities>.lst.FindIndex(x => x.Properties.PortSecurityId == Properties.PortSecurityId); // Locates security in question
                //    if (index > 0 && StaticData<cSecurity, ISecurities>.lst[index].PriceTableCurrenct.Count > 0)
                //    {
                //        m_objPrices.MainData = StaticData<cSecurity, ISecurities>.lst[index].PriceTable;
                //    }
                //    else
                //    {
                //        var priceRepository = Resolver.Resolve<IRepository>();
                //        priceRepository.Execute(session =>
                //        { // Retrieves Datatable of prices of current security, from the repository (database in memory)
                //        m_objPrices.MainData = session.Query<Entities.dbo.Price>().Where(x => x.idSecurity == Properties.PortSecurityId).OrderByDescending(x => x.dDate).ToList();
                //        });


                //        if (index > 0)
                //            StaticData<cSecurity, ISecurities>.lst[index].PriceTable = m_objPrices.MainData; // Updates Prices table of security in question
                //    }
                //}
                return m_objPrices.MainData;
            }
            set { m_objPrices.MainData = value; }
        }//PriceData

        public IRateData RatesClass
        { get { return m_objRates; } }//RatesClass

        public List<Rate> RatesTable
        { get { return m_objRates.RatesData; } }//RatesTable

        public ICovarCorrelData CovarClass
        { get { return m_objCovarData; } }//CovarClass

        public ICovarCorrelData CorrelClass
        { get { return m_objCorrelData; } }//CorrelClass

        public int VolumeIndex
        {
            get { return m_iRecentVolume; }
            set { m_iRecentVolume = value; }
        }//VolumeIndex

        public List<double> Volumes
        {
            get { return m_colVolumes; }
            set { m_colVolumes = value; }
        }//Volumes

        public Boolean isActive
        { get { return m_isActive; } }//isActive

        //public Boolean isValid
        //{ get { return m_isValid; } }//isValid

        public double Weight
        {
            get { return m_objSecAnalytics.Weight; }
            set { m_objSecAnalytics.Weight = value; }
        }//Weight

        public double Quantity
        {
            get { return m_objSecAnalytics.Quantity; }
            set { m_objSecAnalytics.Quantity = value; }
        }//Quantity

        public double LastPrice
        {
            get { return m_objSecAnalytics.LastPrice; }
            set { m_objSecAnalytics.LastPrice = value; }
        }//LastPrice

        public double CapmBeta
        {
            get { return m_objSecAnalytics.CapmBeta; }
            set { m_objSecAnalytics.CapmBeta = value; }
        }//CapmBeta

        public double CapmExpectedReturn
        {
            get { return m_objSecAnalytics.CapmExpectedReturn; }
            set { m_objSecAnalytics.CapmExpectedReturn = value; }
        }//CapmExpectedReturn

        public double FAC
        {
            get { return m_dFAC; }
            set { m_dFAC = value; }
        }//FAC

        public string IdCurrency
        {
            get { return m_idCurrency; }
            set { m_idCurrency = value; }
        }//IdCurrency

        public double AvgYield
        {
            get { return m_dAvgYield; }
            set { m_dAvgYield = value; }
        }//AvgYield

        public double StdYield
        {
            get { return m_dStdYield; }
            set { m_dStdYield = value; }
        }//StdYield

        public double AvgYieldNIS
        {
            get { return m_dAvgYieldNIS; }
            set { m_dAvgYieldNIS = value; }
        }//AvgYieldNIS

        public double StdYieldNIS
        {
            get { return m_dStdYieldNIS; }
            set { m_dStdYieldNIS = value; }
        }//StdYieldNIS

        public double ValueUSA
        {
            get { return m_dValueUSA; }
            set { m_dValueUSA = value; }
        }//ValueUSA

        public double ValueNIS
        {
            get { return m_dValueNIS; }
            set { m_dValueNIS = value; }
        }//ValueNIS

        public double WeightUSA
        {
            get { return m_dWeightUSA; }
            set { m_dWeightUSA = value; }
        }//WeightUSA

        public double WeightNIS
        {
            get { return m_dWeightNIS; }
            set { m_dWeightNIS = value; }
        }//WeightNIS

        public IPortfolioBL Portfolio
        {
            get { return m_objPortfolio; }
            set { m_objPortfolio = value; }
        }

        //public List<Entities.dbo.SecuritiesValues> Values { get; set; }
        #endregion Properties

    } // of class
}
