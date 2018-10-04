using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections;

// Used namespaces
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Collections;
using Cherries.TFI.BusinessLogic.Constraints;
using Cherries.TFI.BusinessLogic.DataManagement;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.DataManagement.Prices;
using Cherries.TFI.BusinessLogic.DataManagement.StaticMethods;
using Cherries.TFI.BusinessLogic.Protection.LicenseManagement;
using Cherries.TFI.BusinessLogic.StaticMethods;
using Cherries.TFI.BusinessLogic.GMath;
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.Optimization;

using TFI.BusinessLogic.Enums;
using Ness.DataAccess.Repository;
using Cherries.Models.App;
using Cherries.Models.ViewModel;
using Cherries.Models.dbo;
using static Cherries.Models.App.PortRiskItem;
using NHibernate.Linq;
using TFI.BusinessLogic.Interfaces;
using TFI.BusinessLogic.Bootstraper;
using Entities = TFI.Entities;

namespace Cherries.TFI.BusinessLogic.Portfolio
{
    public class cPortfolio : IDisposable, IPortfolioBL
    { // The class handles the portfolio file operations

        #region Data Members

        // Main variables
        private IErrorHandler m_objErrorHandler; // Error handler class
        private ICollectionsHandler m_objColHandler; // Collections handler
        private PortfolioDetails m_objPortDetails = new PortfolioDetails(); // Portfolio details class pointer
        private cPortfolioClasses m_objPortfolioClasses = new cPortfolioClasses(); // Portfolio classes instance
        private bool isDisposed = false; // indicates if Dispose has already been called

        // Data variables
        private DataTable m_dtSecsData = new DataTable(); // Securities Datatable
        private DataRow m_drPortDetails; // Portfolio datarow (containing all relevant details)
        private List<Entities.Sp.SecurityData> m_colOpenedSecurities = new List<Entities.Sp.SecurityData>(); // Collection of SecurityData from when opening portfolio
        private int m_iSecsNum = 0; // Number of securities
        private string m_strPriceFldName = "fClose"; // Name of price field (from prices datatable)
        private cPortfolioRiskGrid riskGrid;
        private IRepository repository;
       
        #endregion Data Members

        #region Consturctors, Initialization & Destructor

        public cPortfolio(IErrorHandler cErrors)
        {
            //m_frmMain = fMain;
            m_objErrorHandler = cErrors;
            riskGrid = new StaticMethods.cPortfolioRiskGrid();
            riskGrid.populateInitialCollectionValues();
            cProperties.CurrPortfolio = this;
            
            repository = Resolver.Resolve<IRepository>();

        }//constructor

        ~cPortfolio()
        { Dispose(false); }//destructor

        protected void Dispose(bool disposing)
        { // Disposing class variables
            if (disposing)
            { // Managed code
                m_objErrorHandler = null;
                //m_objColHandler.Dispose();
            }
            isDisposed = true;
        }//Dispose

        public void Dispose()
        { // indicates it was NOT called by the Garbage collector
            Resolver.Release(repository);
            Dispose(true);
            GC.SuppressFinalize(this); // no need to do anything, stop the finalizer from being called
            GC.Collect();
        }//Dispose

        #endregion Consturctors, Initialization & Destructor

        #region Methods

        #region General methods

        public void InitCollectionObject(string currency,  List<int> exchangesPackagees)
        {
            m_objPortfolioClasses.CategoryHandler = new Categories.cCategoriesHandler(m_objErrorHandler);
            m_objColHandler = new cCollectionsHandler(this, currency, exchangesPackagees);

        }//InitCollectionObject


        // TODO: NEW METHOD
        public bool instantiateVariablesForPortfolio(Boolean isCreateNew, string sCurrency, List<int> exchangesIds, List<string> securities = null)
        { // Sets initial values preparing for new / existing portfolio
            try
            {
                // Set categories
                m_objPortfolioClasses.CategoryHandler = new Categories.cCategoriesHandler(m_objErrorHandler);

                // Init Securities collection is cPortfolio.Details
                // TODO: Check if necessary
                if (securities != null && securities.Count > 0)
                { // Initializes collection of securities 
                    this.Details.SecurityData = new List<SecurityData>();
                    foreach (var item in securities)
                        this.Details.SecurityData.Add(new SecurityData { idSecurity = item });
                }

                // Set collections
                Boolean isNewInstance = ((m_objColHandler == null) || (m_objColHandler.Securities.Count == 0));
                if (isNewInstance && (cProperties.CollectionHandler != null))
                    { m_objColHandler = cProperties.CollectionHandler; isNewInstance = false; }

                if (isNewInstance)
                { // Only if collections are empty (first time)
                    m_objColHandler = new cCollectionsHandler(this, sCurrency, exchangesIds);
                    m_objColHandler.loadSecuritiesCollections();
                    cProperties.CollectionHandler = m_objColHandler;
                }
                else m_objColHandler.clearSecsCalculatedOptData();

                // TODO: check if necessary
                cProperties.LastOptimization = DateTime.Today;
                cProperties.isIsraelOnly = sCurrency == "9999";
                
                // Constraints
                if (m_objPortfolioClasses.ConstHandler == null)
                    m_objPortfolioClasses.ConstHandler = new cConstHandler(new cConstraints(), this);

                // Init final variables
                if (m_objPortfolioClasses.PriceHandler == null) m_objPortfolioClasses.PriceHandler = new cPricesHandler(this, Resolver.Resolve<IRepository>()); // Sets prices handler
                if (m_objPortfolioClasses.RatesHandler == null) m_objPortfolioClasses.RatesHandler = new cRateHandler(this);
                if (m_objPortfolioClasses.CovarCorrel == null) m_objPortfolioClasses.CovarCorrel = new TFI.BusinessLogic.GMath.cCovarCorrelHandler(this);
                if (m_objPortfolioClasses.Optimizer == null) m_objPortfolioClasses.Optimizer = new Optimization.cOptimizationResults(this);

                // TODO: in Backtesting  m_objColHandler.ActiveSecs is used and it doesn't have .PriceReturns table filled in!
                if (isNewInstance)
                {
                    m_objPortfolioClasses.RatesHandler.setSecuritiesPriceReturns(m_objColHandler.Securities, DateTime.Today.AddYears(-3).AddDays(-1), DateTime.Today.AddDays(-1), sCurrency);
                    m_objPortfolioClasses.RatesHandler.setSecuritiesPriceReturns(m_objColHandler.Benchmarks, DateTime.Today.AddYears(-3).AddDays(-1), DateTime.Today.AddDays(-1), sCurrency);   
                }

            } catch(Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
            return true;
        }//instantiateVariablesForPortfolio



        //public bool instantiatePortfolioVariables(Boolean isFullProcess, string currency, List<int> exchangesPackagees, List<string> securities = null)
        //{ // Sets variables values for the selected portfolio
        //    try
        //    {
        //        if (isFullProcess)
        //        {
                    
        //            m_objPortfolioClasses.CategoryHandler = new Categories.cCategoriesHandler(m_objErrorHandler);
        //            if (securities != null && securities.Count > 0)
        //            {
        //                this.Details.SecurityData = new List<SecurityData>();
        //                foreach (var item in securities)
        //                {
        //                    this.Details.SecurityData.Add(new SecurityData { idSecurity = item });
        //                }
        //            }
        //            // Security collection
        //            m_objColHandler = new cCollectionsHandler(this, currency, exchangesPackagees);
        //            if ((m_colOpenedSecurities != null) && (m_colOpenedSecurities.Count > 0))
        //                m_objColHandler.setOpenedPortfolioData();
        //            else {
        //                if (!m_objColHandler.setCollectionData()) return false;
        //            }

        //            cProperties.LastOptimization = DateTime.Today;
        //            cProperties.isIsraelOnly = currency == "9999";
        //            // Constraints

        //            if (m_objPortfolioClasses.ConstHandler == null)
        //                m_objPortfolioClasses.ConstHandler = new cConstHandler(new cConstraints(), this);

        //            m_objPortfolioClasses.PriceHandler = new cPricesHandler(this, Resolver.Resolve<IRepository>()); // Sets prices handler
        //            m_objPortfolioClasses.PriceHandler.setPortfolioPricesFromDB();
        //        }
                
                 
        //        setRateCalcHandler();
        //        m_objPortfolioClasses.CovarCorrel = new TFI.BusinessLogic.GMath.cCovarCorrelHandler(this);
        //        //m_objPortfolioClasses.EfficientFrontier = new Classes.GMath.EF.cEFHandler(fMain, this);
        //        m_objPortfolioClasses.Optimizer = new Optimization.cOptimizationResults(this);
        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex);
        //    }
        //    return true;
        //}//instantiatePortfolioVariables

        
        private void setRateCalcHandler()
        { // Initializes rate handler
            //if (m_objPortfolioClasses.RatesHandler != null) m_objPortfolioClasses.RatesHandler.Dispose();
            m_objPortfolioClasses.RatesHandler = new cRateHandler(this);
        }//setRateCalcHandler

        public void openExistingPortfolio(int iPortID, bool isBacktPort)
        {
            if (!isBacktPort)
                fillPortDetails<Entities.dbo.Portfolio>(iPortID, isBacktPort);
            else
                fillPortDetails<Entities.dbo.BacktestingPortfolio>(iPortID, isBacktPort);
        }//openExistingPortfolio

       
        public cPortRiskItem GetRisk(double risk)
        {
            return riskGrid.getPortRiskItem(risk);
        }
        #endregion General methods
        
        #region DB operations
        
        private void fillPortDetails<T>(int iPortID, bool isBacktPort) where T : Entities.dbo.PortfolioBase
        { // Loads portfolio data from DB
            try
            {
                double dSecValue = 0;
                double dCash = 0;
                List<Entities.Sp.SecurityData> securities = new List<Entities.Sp.SecurityData>(); 

                repository.Execute(session =>
                {
                    var portfolio = session.Get<T>(iPortID);
                    m_objPortDetails = AutoMapper.Mapper.Map<PortfolioDetails>(portfolio);

                    if (portfolio.dInitRisk.HasValue)
                        m_objPortDetails.PreferedRisk = riskGrid.getPortRiskItem((double)portfolio.dInitRisk);
                    if (portfolio.dCurrRisk.HasValue)
                        m_objPortDetails.CurrentStDev = Convert.ToDouble(portfolio.dCurrRisk);
                    else m_objPortDetails.CurrentStDev = m_objPortDetails.PreferedRisk.UpperBound;
                    m_objPortDetails.CalcCurrency = portfolio.CalcCurrency;

                    GetPortfolioSectorDist();

                    Dictionary<string, Tuple<object, NHibernate.Type.IType>> param = new Dictionary<string, Tuple<object, NHibernate.Type.IType>>();
                    param.Add("Id", new Tuple<object, NHibernate.Type.IType>(m_objPortDetails.ID, NHibernate.NHibernateUtil.Int32));

                    if (isBacktPort)
                        securities = repository.ExecuteSp<Entities.Sp.SecurityData>("dataGetBacktestingPortfolioSecurities", param).ToList();
                    else
                        securities = repository.ExecuteSp<Entities.Sp.SecurityData>("dataGetPortfolioSecurities", param).ToList();

                    DateTime calcDate = DateTime.Today.AddDays(-1); //LR: If using below calculations for BTesting, it should be another date here
                    var m_AdjCoeff = (portfolio.CalcCurrency == "9999") ? 100.0 : 1;
                    m_objPortfolioClasses.PriceHandler = new cPricesHandler(this, Resolver.Resolve<IRepository>());
                    m_objPortDetails.SecurityData = AutoMapper.Mapper.Map<List<SecurityData>>(securities);


                    //m_objPortDetails.DateCreated
                    if (!isBacktPort)
                    {
                        foreach (var sec in m_objPortDetails.SecurityData)
                        {
                            if (sec.idSecurityType != 106)
                            {
                                // TODO: replace this function with something else - 
                                //var price = m_objPortfolioClasses.PriceHandler.GetPrice(sec.idSecurity, portfolio.CalcCurrency, calcDate);
                                //var dPrice = (price != null && price.RateVal.HasValue ? price.RateVal.Value : 0);

                                double dCreateDateValue = sec.portSecWeight * m_objPortDetails.Equity;
                                double dCurrentDateValue = sec.portSecWeight * m_objPortDetails.CurrEquity;
                                sec.SecValue = dCurrentDateValue; // Calculates value based on weight
                                sec.flQuantity = (sec.SecValue * m_AdjCoeff) / sec.flYesterdayPrice; // Updates quantity
                                dSecValue = Math.Floor(sec.flQuantity) * sec.flYesterdayPrice / m_AdjCoeff;
                                dCash += (sec.flQuantity - Math.Floor(sec.flQuantity)) * sec.flYesterdayPrice / m_AdjCoeff;
                                dCash += dSecValue - Math.Floor(dSecValue); //cents/agorot
                                sec.SecValue = Math.Floor(dSecValue);
                                //sec.Profit = dCurrentDateValue / dCreateDateValue - 1;
                                sec.Profit = sec.flYesterdayPrice / sec.flCreationPrice - 1; // flCreationPrice is price on (dtCreated - 1)
                            }
                        }
                        m_objPortDetails.Cash = dCash;
                    }
                });

                m_colOpenedSecurities = securities;  // Looks like it is only used for Backtesting
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//loadNewPortfolio
        
        private void GetPortfolioSectorDist()
        {
            repository.Execute(session =>
            {
                var portfolioQry = session.Query<Entities.dbo.PortfolioSecurities>().Where(x => x.Portfolios.idPortfolio == m_objPortDetails.ID);

                var sectors = portfolioQry.GroupBy(x => x.Securities.Sectors.strName)
                                      .Select(x => new Cherries.Models.Lookup.Sector
                                      {
                                          Name = x.Key,
                                          Weights = Convert.ToDouble(x.Sum(s => s.flWeight) * 100)
                                      }).ToList();
                m_objPortDetails.PortfolioSectors = sectors;
            });
        }
        #endregion DB operations

        #endregion Methods

        #region Properties

        public PortfolioDetails Details
        {
            get { return m_objPortDetails; }
            set { m_objPortDetails = value; }
        }//Details

        public ICollectionsHandler ColHandler
        {
            get { return m_objColHandler; }
            set { m_objColHandler = value; }
        }//ColHandler
 
        public IErrorHandler cErrorLog
        { get { return m_objErrorHandler; } }//cErrorLog

        public cPortfolioClasses Classes
        {
            get { return m_objPortfolioClasses; }
            set { m_objPortfolioClasses = value; }
        }//Classes

        //public static ctlConstraints ConstraintsControl
        //{ get { return m_ctlConsts; } }//ConstraintsControl
       
        public DataTable SecuritiesTable
        { get { return m_dtSecsData; } }//SecuritiesTable

        public DataRow PortRow
        { 
            get { return m_drPortDetails; }
            set { m_drPortDetails = value; }
        }//PortRow

        public List<Entities.Sp.SecurityData> OpenedSecurities
        { get { return m_colOpenedSecurities; } }//OpenedSecurities

        public String PricesFld
        {
            get { return m_strPriceFldName; }
            set { m_strPriceFldName = value; }
        }//PricesFld

        public int SecsNum
        {
            get { return m_iSecsNum; }
            set { m_iSecsNum = value; }
        }//SecsNum

        #endregion Properties

    }//of class
}
