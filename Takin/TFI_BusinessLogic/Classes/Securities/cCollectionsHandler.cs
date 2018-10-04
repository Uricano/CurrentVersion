using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

// Used namespaces
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.DataManagement;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.DataManagement.ImportPorts;
using TFI.BusinessLogic.Enums;
using Cherries.TFI.BusinessLogic.Portfolio;
using Cherries.TFI.BusinessLogic.Categories;
using Ness.DataAccess.Repository;
using Ness.DataAccess.Fluent;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NHibernate.Linq;
using TFI.BusinessLogic.Interfaces;
using TFI.BusinessLogic.Bootstraper;
using Entities = TFI.Entities;
using Cherries.TFI.BusinessLogic.Collections;
using System.Web;
using TFI.Entities.Lookup;
using TFI.Entities.Sp;
using Cherries.Models.dbo;
using Cherries.Models.App;
using NHibernate.SqlCommand;
using System.Diagnostics;

namespace Cherries.TFI.BusinessLogic.Collections
{
    public class cCollectionsHandler : ICollectionsHandler, IDisposable
    {

        #region Data members

        // Main variables
        private IPortfolioBL m_objPortfolio; // Portfolio class pointer
        private ICategoriesHandler m_objCatHandler; // Category handler class
        private IErrorHandler m_objErrorHandler; // Error handler class
        private bool isDisposed = false; // Dispose indicator
        private string currency;
        private List<int> exchanges;
        private List<String> m_colPrevStateOfSecurities = new List<String>(); // List of the previous state of the securities (by SecId)

        // Data variables
        private ISecurities m_colFullCollection; // Full collection of securities
        private ISecurities m_colSecurities; // Securities Collection
        private ISecurities m_colSecuritiesByRisk; // Securities defined by a given risk level
        private ISecurities m_colActiveSecurities; // Active securities collection
        private ISecurities m_colDisabledSecurities; // Collection of securities that has been disabled in this round
        private ISecurities m_colBenchmarks;         // Collection of Benchmarks used in Backtesting (LocalIDs: 137, 143, 142, 709, 0.5177, 0.2020, 0.1297, 2, 147, 168, 164)
        private ICategoryCollection m_colSectors; // Collection of sectors
        private ICategoryCollection m_colMarkets; // Collection of exchanges
        private ICategoryCollection m_colSecTypes; // Collection of security types
        private Boolean m_isPortSec = false; // Whether a portfolio security had been created
        private Boolean m_isModifiedSelection = true; // Whether  the user has modified the selection in the current session

        //private TFI_CS_Shared.ResultCodes m_enumErrorCode;  // Code for status in server requests
        private String m_strErrorMsg = "";                  // Message from call to server

        private IRepository securityRepository;

        private static List<Task> listTask = new List<Task>();

        private static object lockObject = new object();
        #endregion Data members

        #region Constructors, Initialization & Destructors

        public cCollectionsHandler(IPortfolioBL cPort, string currency, List<int> exchangesPackagees)
        {
            m_objPortfolio = cPort;
            m_objCatHandler = m_objPortfolio.Classes.CategoryHandler;
            m_objErrorHandler = m_objPortfolio.cErrorLog;
            m_colFullCollection = new cSecurities(m_objPortfolio);
            m_colSecurities = new cSecurities(m_objPortfolio);
            m_colSecuritiesByRisk = new cSecurities(m_objPortfolio);
            m_colActiveSecurities = new cSecurities(m_objPortfolio);
            m_colDisabledSecurities = new cSecurities(m_objPortfolio);
            m_colBenchmarks = new cSecurities(m_objPortfolio);
            this.currency = currency;
            this.exchanges = exchangesPackagees;

            //LR:TODO
            ////if (exchanges == null)
            ////    exchanges = new List<int>(new int[]{ 1, 3, 4, 5 });    

            securityRepository = Resolver.Resolve<IRepository>();
        }//constructor

        private void initCategoryCollections()
        { // Initializes the category collections (to empty lists)
            m_colSectors = new cCategoryCollection(enumCatType.Sector, m_objErrorHandler);
            m_colSecTypes = new cCategoryCollection(enumCatType.SecurityType, m_objErrorHandler);
            m_colMarkets = new cCategoryCollection(enumCatType.StockMarket, m_objErrorHandler);
        }//initCategoryCollections

        public void Dispose(bool disposing)
        { // Disposing class variables
            if (disposing)
            { // Managed code
                Resolver.Release(securityRepository);
                m_objErrorHandler = null;
                m_colFullCollection.Clear();
                m_colSecurities.Clear();
                m_colActiveSecurities.Clear();
                m_colBenchmarks.Clear();
                m_colSectors.Clear();
                m_colSecTypes.Clear();
                m_colMarkets.Clear();
            }
            isDisposed = true;
        }//Dispose

        public void Dispose()
        { // Clear from memory
            Dispose(true);
            GC.SuppressFinalize(this);
        }//Dispose

        #endregion Constructors, Initialization & Destructors

        #region Methods

        #region Filter Methods

        public void filterSecuritiesForNewPortfolio(List<int> colExchanges)
        { // Filters collection of securities based on the user's preferences
            for (int iSecs = 0; iSecs < m_colSecurities.Count; iSecs++)
                if (!colExchanges.Contains(m_colSecurities[iSecs].Properties.Market.ID))
                    m_colSecurities[iSecs].disableCurrentSecurity();

            

        }//filterSecuritiesForNewPortfolio

        #endregion Filter Methods

        #region General

        public void setDisabledSecsToActive()
        { // Sets disabled securities back to active state

            // Loop through list of disabled securities and setting them to active state
            for (int iSecs = 0; iSecs < m_colDisabledSecurities.Count; iSecs++)
                m_colDisabledSecurities[iSecs].setSecurityActivity(true);

            // Clear DisabledSecs collection
            m_colDisabledSecurities.Clear();

            // Clear DisabledSecs collection
            m_colActiveSecurities = m_colFullCollection.getListOfActiveSecs(); // Creates active securities list
        }//setDisabledSecsToActive

        public bool loadSecuritiesCollections()
        {// Sets all necessary collections of securities
            try
            {
                if ((m_colSectors == null) || (m_colSectors.getCount() == 0))
                    initCategoryCollections(); // Creates empty collections

                // Fill collections
                fillCollectionsFromStaticData();
                setSubCollections();
                initSelCollections();

                // Benchmarks
                m_colBenchmarks = StaticData<cSecurity, ISecurities>.BenchMarks;

            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
                return false;
            }
            return (m_colFullCollection.Count > 0);
        }//loadSecuritiesCollections

        public void clearSecsCalculatedOptData()
        { // Clears the data of the security that has been calculated in the previous calculation
            for (int iSecs = 0; iSecs < m_colSecurities.Count; iSecs++)
                m_colSecurities[iSecs].clearOptimizationData();
        }//clearSecsCalculatedData

        private void setSubCollections()
        { // Sets the smaller collections (of active secs and benchmarks)
            setListOfSecuritiesByRisk();
            m_colSecurities.sortSecurities();
            m_colFullCollection.sortSecurities();

            m_colActiveSecurities = m_colFullCollection.getListOfActiveSecs(); // Creates active securities list
        }//setSubCollectionsNoCategories

        public void resetSecuritiesCollection()
        { // Resets collection of securities to be full
            m_colSecurities.Clear();
            for (int iSecs = 0; iSecs < m_colFullCollection.Count; iSecs++)
                m_colSecurities.Add(m_colFullCollection[iSecs]);
        }//resetSecuritiesCollection

        private void setListOfSecuritiesByRisk()
        { // Creates list of securities by a given risk level (defined in cProperties)
            m_colSecuritiesByRisk.Clear();
            for (int iSecs = 0; iSecs < m_colSecurities.Count; iSecs++)
                if (m_colSecurities[iSecs].StdYield <= m_objPortfolio.Details.PreferedRisk.stDevUpperBound)
                    m_colSecuritiesByRisk.Add(m_colSecurities[iSecs]);
        }//setListOfSecuritiesByRisk

        private void initSelCollections()
        { // Sets the securities corresponding to each Sel collection
            m_colSectors.setProperSecuritiesCollections(m_colSecurities);
            m_colSecTypes.setProperSecuritiesCollections(m_colSecurities);
            m_colMarkets.setProperSecuritiesCollections(m_colSecurities);
        }//initSelCollections

        public ICategoryItem getCategoryItemByName(String strName, enumCatType eType)
        { return getCategoryCollectionByType(eType).getItemByName(strName); }//getCategoryItemByName

        private ICategoryCollection getCategoryCollectionByType(enumCatType eType)
        { // Returns the desired category collection
            switch (eType)
            {
                case enumCatType.StockMarket: return m_colMarkets;
                case enumCatType.SecurityType: return m_colSecTypes;
                default: return m_colSectors;
            }
        }//getCategoryCollectionByType

        public void clearCollection()
        {
            m_colFullCollection.Clear();
            m_colSecurities.Clear();
            m_colActiveSecurities.Clear();
        }//clearCollection

        #endregion General

        #region Load from DB

        public void SetSecurites()
        { // Sets initial collection of securities (from DB to static data)
            initCategoryCollections();
            //IList<Entities.Lookup.Selexchanges> exchanges = null;
            IList<SelStockExchange> exchangesToUpdate = null;
            DateTime updateDate = DateTime.Now;
            bool updateBenchMarks = false;
            //Dictionary<int, DateTime> dicUpdatedExchanges = null;
            if (StaticData<cSecurity, ISecurities>.dicUpdatedExchanges == null)
                StaticData<cSecurity, ISecurities>.dicUpdatedExchanges = new Dictionary<int, DateTime>();
            //else
            // dicUpdatedExchanges = StaticData<cSecurity, ISecurities>.dicUpdatedExchanges;

            //LR:TODO
            securityRepository.Execute(session =>
            {
                if (exchanges == null)
                    exchanges = session.Query<Entities.Lookup.SelStockExchange>().Where(x => x.IsActive).Select(x => x.id).ToList();
                //if (string.IsNullOrEmpty(this.currency)) this.currency = session.Get<Entities.Lookup.Parameters>("CurrencyId").Value;
                exchangesToUpdate = session.Query<SelStockExchange>().ToList();
            });

            // Get prices - TAKES LONG TIME
            //List<Models.dbo.Price> ALLprices = getFullPrices();
            //StaticData<cSecurity, ISecurities>.ALLprices = ALLprices;

            // Loads securities to memory by exchanges
            HashSet<int> exchangesInts = new HashSet<int>();
            foreach (var code in exchanges)
            {
                DateTime exchangeLastUpdate = DateTime.MinValue;
                StaticData<cSecurity, ISecurities>.dicUpdatedExchanges.TryGetValue(code, out exchangeLastUpdate);


                if (exchangesInts.Add(code)  && exchangesToUpdate.Any(x => x.id == code))  //LR:TODO
                {
                    AddSecuritiesToMemory(code);     //, ALLprices);
                    updateDate = DateTime.Now;
                    if (StaticData<cSecurity, ISecurities>.dicUpdatedExchanges.ContainsKey(code))
                        StaticData<cSecurity, ISecurities>.dicUpdatedExchanges[code] = updateDate;
                    else
                        StaticData<cSecurity, ISecurities>.dicUpdatedExchanges.Add(code, updateDate);

                    updateBenchMarks = true;
                }
            }    
            
            if (updateBenchMarks)
                SetSecurityBenchMarks();
            //HttpContext.Current.Application.Add("dicUpdatedExchanges", dicUpdatedExchanges);

        }//SetSecurites

        public void AddSecuritiesToMemory(int exchange)     //, List<Models.dbo.Price> ALLprices)
        {
            securityRepository.Execute(session =>
            {
                //List<cSecurity> lst;
                if (StaticData<cSecurity, ISecurities>.lst == null)
                {
                    StaticData<cSecurity, ISecurities>.lst = new List<cSecurity>();
                }
                else
                {
                    //lst = StaticData<cSecurity, ISecurities>.lst;
                    var secs = StaticData<cSecurity, ISecurities>.lst.Where(x => x.Properties.Market.ID == exchange).ToList();
                    int i = 0;
                    while (secs.Count() > 0)
                    {
                        StaticData<cSecurity, ISecurities>.lst.Remove(secs[i]);
                        secs.Remove(secs[i]);
                    }
                }
                var securities = m_colSecurities.GetTopSecurities(new List<int>() { exchange }, new List<int>() { 1 });

                m_colSecurities.Clear();
                m_colFullCollection.Clear();

                string securityID_list = getSecIDsString(securities);

                List<Models.dbo.Price> ALLprices = getFullPrices(securityID_list);

                var parallelList = new ConcurrentBag<cSecurity>();
                Parallel.ForEach(securities, s =>
                {
                    try
                    {
                        if (StaticData<cSecurity, ISecurities>.lst.Where(x => x.Properties.PortSecurityId == s.idSecurity).Count() == 0)
                        {
                            var CurrentSec = getCurrSecurity(s, m_objPortfolio);

                            CurrentSec.PriceTable = AutoMapper.Mapper.Map<List<Price>, List<Entities.dbo.Price>>(ALLprices.Where(x => x.idSecurity == CurrentSec.Properties.PortSecurityId).OrderByDescending(x => x.dDate).ToList());

                            if (CurrentSec != null)
                                parallelList.Add(CurrentSec);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (m_objErrorHandler != null) m_objErrorHandler.LogInfo(ex);
                    }
                });
                StaticData<cSecurity, ISecurities>.lst.AddRange(parallelList);
            });
        }

        private string getSecIDsString(List<Security> securities)
        {
            string strSecIDsString = "";

            foreach (var item in securities)
            {
                strSecIDsString += "," + item.idSecurity;
            }
            if (strSecIDsString != "")
                strSecIDsString = strSecIDsString.Substring(1); // get rid of first ','

            return strSecIDsString;
        }

        public void addMissingSecurities(ISecurities newColl, List<string> missingSecs)
        {
            securityRepository.Execute(session =>
            {
                var securities = newColl.GetTopSecurities(new List<int>() { 1 }, new List<int>() { 1 }, string.Join(",", missingSecs));
                List<Models.dbo.Price> ALLprices = getFullPrices(string.Join(",", missingSecs));

                Parallel.ForEach(securities, s =>
                {
                    try
                    {
                            var CurrentSec = getCurrSecurity(s, m_objPortfolio);

                            CurrentSec.PriceTable = AutoMapper.Mapper.Map<List<Price>, List<Entities.dbo.Price>>(ALLprices.Where(x => x.idSecurity == CurrentSec.Properties.PortSecurityId).OrderByDescending(x => x.dDate).ToList());

                            if (CurrentSec != null)
                                newColl.Add(CurrentSec);
                    }
                    catch (Exception ex)
                    {
                        if (m_objErrorHandler != null) m_objErrorHandler.LogInfo(ex);
                    }
                });

                m_objPortfolio.Classes.RatesHandler.setSecuritiesPriceReturns(newColl, DateTime.Today.AddYears(-3).AddDays(-1), DateTime.Today.AddDays(-1), m_objPortfolio.Details.CalcCurrency);

            });
        }

        public List<cSecurity> convertListToCSecurity(List<Security> colOrigSecs)
        { // Converts list of Security objects to a list of cSecurity objects
            List<cSecurity> colFinalSecs = new List<cSecurity>();

            //List<Models.dbo.Price> colPrices = getFullPrices();
            try
            {
                for (int iSecs = 0; iSecs < colOrigSecs.Count; iSecs++)
                {
                    cSecurity cCurrSec = getCurrSecurity(colOrigSecs[iSecs], m_objPortfolio);
                    if (cCurrSec != null)
                        colFinalSecs.Add(cCurrSec);
                }
            }
            catch (Exception ex)
            {
                m_objErrorHandler.LogInfo(ex);
            }
            return colFinalSecs;
        }//convertListToCSecurity


        public List<Models.dbo.Price> getFullPrices(string security_list = "ALL")
        { // Receives full collection of prices
            Dictionary<string, Tuple<object, NHibernate.Type.IType>> param = new Dictionary<string, Tuple<object, NHibernate.Type.IType>>();
            param.Add("start_date", new Tuple<object, NHibernate.Type.IType>(DateTime.Today.AddDays(-1).AddYears(-3), NHibernate.NHibernateUtil.DateTime));
            param.Add("end_date", new Tuple<object, NHibernate.Type.IType>(DateTime.Today.AddDays(-1), NHibernate.NHibernateUtil.DateTime));
            if (security_list != "ALL")
                param.Add("security_list", new Tuple<object, NHibernate.Type.IType>(security_list, NHibernate.NHibernateUtil.StringClob));

            // List<Models.dbo.Price> mainPrices = securityRepository.ExecuteSp<Models.dbo.Price>("getFullPrices", param).GroupBy(x => x.idSecurity).Select(g => g.First()).OrderBy(x => x.dDate).ToList();
            List<Models.dbo.Price> mainPrices = securityRepository.ExecuteSp<Models.dbo.Price>("getFullPrices", param).ToList();
            return AutoMapper.Mapper.Map<List<Models.dbo.Price>>(mainPrices);
        }//getFullPrices

        private void SetSecurityBenchMarks()
        {
            try
            {
                //List<String> BenchmarksIDlist = new List<string>() { "137", "143", "142", "709", "0.5177", "0.1297", "2", "147" };
                //securityRepository.Execute(session =>
                //{
                //    var securities = session.Query<Entities.dbo.Security>().Where(x => BenchmarksIDlist.Contains(x.idSecurity));


                    var securities = m_colSecurities.GetBMSecurities();

                    List<Models.dbo.BMPrice> BMprices = m_colSecurities.GetBMPrices();

                //BMprices.whe

                    cSecurity cCurrSec;
                    foreach (var item in securities)
                    {
                        cCurrSec = getCurrBMSecurity(item, m_objPortfolio);
                        if (cCurrSec != null)
                        {
                            //Add prices
                            // Made separate entity for tbl_IndexPrices
                            //var priceRepository = Resolver.Resolve<IRepository>();
                            //List<BMPrice> laura;
                            try
                            {
                            //priceRepository.Execute(session =>
                            //{
                            //laura = BMprices.Where(x => x.idSecurity == cCurrSec.Properties.PortSecurityId).OrderByDescending(x => x.dDate).ToList();

                            //laura = BMprices.Where(y => y.idSecurity == cCurrSec.Properties.PortSecurityId).ToList();


                            cCurrSec.PriceTable = AutoMapper.Mapper.Map<List<BMPrice>, List<Entities.dbo.Price>>(BMprices.Where(x => x.idSecurity == cCurrSec.Properties.PortSecurityId).OrderByDescending(x => x.dDate).ToList());
                            //cCurrSec.PriceTable = AutoMapper.Mapper.Map<List<BMPrice>, List<Entities.dbo.Price>>(laura);
                            
                            
                            // laura = session.Query<Entities.dbo.BMPrice>().Where(x => x.idSecurity == "'" + cCurrSec.Properties.PortSecurityId + "'").OrderByDescending(x => x.dDate).ToList();
                            //});
                        }
                        catch (Exception ex)
                            {
                                m_objErrorHandler.LogInfo(ex);
                            }
                            //Resolver.Release(priceRepository);


                            m_colBenchmarks.Add(cCurrSec);
                        }
                    }
                    //HttpContext.Current.Application.Add("BenchMark", m_colBenchmarks);
                    StaticData<cSecurity, ISecurities>.BenchMarks = m_colBenchmarks;
                //});
            }
            catch (Exception ex)
            {
                if (m_objErrorHandler != null) m_objErrorHandler.LogInfo(ex);
            }

            //});

        }

        private void fillCollectionsFromStaticData()
        { // Loads collections of securities from the static data 
            try
            {
                // Init
                if (StaticData<cSecurity, ISecurities>.lst == null)
                { // init Static collection if it is empty
                    DateTime lastUpdate = DateTime.MinValue;
                    SetSecurites();
                }

                //////// Compare to portfolio securities 
                //////List<cSecurity> lstSecs = null;


                //*****************************************************************************
                //var portfolioSecurities = m_objPortfolio.Details.SecurityData.Select(x => x.idSecurity).ToList();
                //if ((portfolioSecurities == null) || (portfolioSecurities.Count == 0))
                //*****************************************************************************


                //////lstSecs = StaticData<cSecurity, ISecurities>.lst.Where(x => exchanges.Contains(x.Properties.Market.ID)).ToList();


                //*****************************************************************************
                // Merge collections
                //if (lstSecs != null)
                //{ // Portfolio securities found = we limit our collections to portfolio securities
                //    //lstSecs = new List<TFI.BusinessLogic.Securities.cSecurity>();
                //    lstSecs = (from l in StaticData<cSecurity, ISecurities>.lst
                //               join li in portfolioSecurities
                //               on l.Properties.PortSecurityId equals li
                //               select l).ToList();

                //    var notInMemory = portfolioSecurities.Where(x => !lstSecs.Any(s => s.Properties.PortSecurityId == x)).ToList();
                //    var parallelList = new ConcurrentBag<cSecurity>();
                //    List<Security> secs = new List<Security>();
                //    securityRepository.Execute(session =>
                //    {
                //        secs = AutoMapper.Mapper.Map<List<Security>>(session.Query<Entities.dbo.TopSecurities>().Where(x => notInMemory.Contains(x.idSecurity)));
                //    });
                //    Parallel.ForEach(secs, s =>
                //    {
                //        var CurrentSec = getCurrSecurity(s, m_objPortfolio);
                //        parallelList.Add(CurrentSec);
                //    });
                //    lstSecs.AddRange(parallelList);
                //}//main if
                //*****************************************************************************

                // Set final collections
                m_colFullCollection.Securities.AddRange(StaticData<cSecurity, ISecurities>.lst);    // (lstSecs);
                m_colSecurities.Securities.AddRange(StaticData<cSecurity, ISecurities>.lst);    // (lstSecs);
                m_colActiveSecurities = m_colSecurities.getListOfActiveSecs();      //LR: at this point all securities are active.

                for (int i = 0; i < m_colSecurities.Securities.Count; i++)
                { // Init collection securities
                    m_colSecurities.Securities[i].Init(m_objPortfolio);
                    m_colFullCollection.Securities[i].Init(m_objPortfolio);
                }
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//fillCollectionsFromStaticData

        private void loadSecuritiesFromDb()
        { // Loads the portfolio securities from database
          //session.Query(ses =>
          //{
            try
            {
                //List<cSecurity> lst = null;
                if (StaticData<cSecurity, ISecurities>.lst == null)
                {
                    DateTime lastUpdate = DateTime.MinValue;
                    SetSecurites();
                }

                //lst = StaticData<cSecurity, ISecurities>.lst;
                List<cSecurity> lstSecs = new List<TFI.BusinessLogic.Securities.cSecurity>();
                
                var limitedSecurities = m_objPortfolio.Details.SecurityData.Select(x => x.idSecurity).ToList();
                if (limitedSecurities != null && limitedSecurities.Count > 0)
                {
                    lstSecs = (from l in StaticData<cSecurity, ISecurities>.lst
                             join li in limitedSecurities
                             on l.Properties.PortSecurityId equals li
                             select l).ToList();
                    var notInMemory = limitedSecurities.Where(x => !lstSecs.Any(s => s.Properties.PortSecurityId == x)).ToList();
                    var parallelList = new ConcurrentBag<cSecurity>();
                    List<Security> secs = new List<Security>();
                    securityRepository.Execute(session =>
                    {
                        secs = AutoMapper.Mapper.Map<List<Security>>(session.Query<Entities.dbo.TopSecurities>().Where(x => notInMemory.Contains(x.idSecurity)));
                    });
                    Parallel.ForEach(secs, s =>
                    {
                        var CurrentSec = getCurrSecurity(s, m_objPortfolio);
                        parallelList.Add(CurrentSec);
                    });
                    lstSecs.AddRange(parallelList);
                }
                else
                {
                    lstSecs = StaticData<cSecurity, ISecurities>.lst.Where(x => exchanges.Contains(x.Properties.Market.ID)).ToList();
                }

                m_colFullCollection.Securities.AddRange(lstSecs);
                
                m_colSecurities.Securities.AddRange(lstSecs);
               
                for (int i = 0; i < m_colSecurities.Securities.Count; i++)
                {
                    m_colSecurities.Securities[i].Init(m_objPortfolio);
                    m_colFullCollection.Securities[i].Init(m_objPortfolio);
                }
            }
            catch (Exception ex)
            {
                m_objErrorHandler.LogInfo(ex);
            }

            //});

        }//loadSecuritiesFromDb

        private void loadSecuritiesFromCollection(List<Entities.Sp.SecurityData> securities)
        { // Fills collection of securities from a given collection
            List<cSecurity> cFinalSecs = new List<cSecurity>();

            for (int iSecs = 0; iSecs < securities.Count; iSecs++)
            { // Goes through list of securities
                cSecurity cCurrSec = new cSecurity(m_objPortfolio, securities[iSecs].strName, securities[iSecs].strSymbol);
                cCurrSec.AvgYield = securities[iSecs].avgYield;
                cCurrSec.AvgYieldNIS = securities[iSecs].avgYieldNIS;
                cCurrSec.DateRange = new cDateRange((DateTime)m_objPortfolio.Details.StartDate, (DateTime)m_objPortfolio.Details.EndDate);
                cCurrSec.IdCurrency = securities[iSecs].idCurrency;
                cCurrSec.Quantity = securities[iSecs].flQuantity;
                cCurrSec.StdYield = securities[iSecs].stdYield;
                cCurrSec.StdYieldNIS = securities[iSecs].stdYieldNIS;
                cCurrSec.Weight = securities[iSecs].portSecWeight;
                cCurrSec.WeightNIS = securities[iSecs].WeightNIS;
                cCurrSec.WeightUSA = securities[iSecs].WeightUSA;

                cCurrSec.Properties.HebName = securities[iSecs].strHebName;
                cCurrSec.Properties.PortSecurityId = securities[iSecs].idSecurity;
                cCurrSec.Properties.Market = new cCategoryItem(enumCatType.StockMarket, securities[iSecs].marketName, securities[iSecs].idMarket, m_objErrorHandler, m_objPortfolio);
                cCurrSec.Properties.MarketName = securities[iSecs].marketName;
                cCurrSec.Properties.Sector = new cCategoryItem(enumCatType.Sector, securities[iSecs].sectorName, securities[iSecs].idSector, m_objErrorHandler, m_objPortfolio);
                cCurrSec.Properties.SecurityType = new cCategoryItem(enumCatType.SecurityType, securities[iSecs].securityTypeName, securities[iSecs].idSecurityType, m_objErrorHandler, m_objPortfolio);

                cFinalSecs.Add(cCurrSec);
            }

            m_colFullCollection.Securities.AddRange(cFinalSecs);

            m_colSecurities.Securities.AddRange(cFinalSecs);
            //List<cSecurity> cFinalSecs = AutoMapper.Mapper.Map<List<cSecurity>>(securities);

        }//loadSecuritiesFromCollection

        public ICategoryItem getCatItemByID(enumCatType eType, int iId, ICategoryCollection cCollection)
        { // Retrieves category Item From given category ID
            ICategoryItem cCurrItem = cCollection.getItemByID(iId);
            if (cCurrItem == null) // new item
            { // Doesn't exist - create new
                cCurrItem = new cCategoryItem(eType, m_objCatHandler.getCategoryCol(eType).getCategoryVal(iId), iId, m_objErrorHandler, m_objPortfolio);
                cCollection.Add(cCurrItem);
            }
            return cCurrItem;
        }//getCatItemByID

        #endregion Load from DB
        

        #region Static methods

        private cSecurity getCurrSecurity(Security security, IPortfolioBL cCurrPort)
        { // Retrieves an instance of a security based on its datarow info
            ICollectionsHandler cColHandler = cCurrPort.ColHandler;

            cSecurity cCurrSec = new cSecurity(cCurrPort, security.strName, security.strSymbol);//111An
            cCurrSec.Properties.PortSecurityId = security.idSecurity;

            cCurrSec.Properties.HebName = security.strHebName;

            //cCurrSec.FAC = Convert.ToDouble(security.FAC);

            if (cCurrSec.FAC <= 0D) cCurrSec.FAC = 1D;

            cCurrSec.AvgYield = security.AvgYield; //* 52;
            cCurrSec.StdYield = security.StdYield;// * Math.Sqrt(52);

            cCurrSec.AvgYieldNIS = security.AvgYieldNIS;// * 52;
            cCurrSec.StdYieldNIS = security.StdYieldNIS;// * Math.Sqrt(52);

            cCurrSec.ValueUSA = security.dValueUSA;
            cCurrSec.ValueNIS = security.dValueNIS;

            cCurrSec.WeightUSA = security.WeightUSA;
            cCurrSec.WeightNIS = security.WeightNIS;

            //cCurrSec.Properties.ISIN = security.strISIN;
            cCurrSec.IdCurrency = security.idCurrency;
            cCurrSec.DateRange = new cDateRange(security.dtPriceStart, security.dtPriceEnd);
            cCurrSec.setSecurityActivity(true);

            lock (lockObject)
            {
                cCurrSec.Properties.Sector = cColHandler.getCatItemByID(enumCatType.Sector, security.idSector,
                        cColHandler.Sectors);

                cCurrSec.Properties.Market = cColHandler.getCatItemByID(enumCatType.StockMarket, security.idMarket, cColHandler.Markets);
                cCurrSec.Properties.MarketName = getSecMarketName(security.idMarket);

                cCurrSec.Properties.SecurityType = cColHandler.getCatItemByID(enumCatType.SecurityType,
                       security.idSecurityType, cColHandler.SecTypes);
            }


            //var priceRepository = Resolver.Resolve<IRepository>();
            //try
            //{
            //    priceRepository.Execute(session =>
            //    {
            //        cCurrSec.PriceTable = session.Query<Entities.dbo.Price>().Where(x => x.idSecurity == security.idSecurity).ToList(); //.OrderByDescending(x => x.dDate)
            //    });
            //}
            //catch (Exception ex)
            //{
            //    m_objErrorHandler.LogInfo(ex);
            //}
            //Resolver.Release(priceRepository);


            //List<Models.dbo.Price> BMprices = getFullPrices();
            //cCurrSec.PriceTable = AutoMapper.Mapper.Map<List<Price>, List<Entities.dbo.Price>>(BMprices.Where(x => x.idSecurity == cCurrSec.Properties.PortSecurityId).OrderByDescending(x => x.dDate).ToList());


            return cCurrSec;
        }//getCurrSecurity

        private String getSecMarketName(int idMarket)
        { // Gets the market name for display (for the current security)
            switch(idMarket)
            {
                case 1: return "TASE";
                case 3: return "NASDAQ";
                case 4: return "NYSE";
                case 5: return "AMEX";
            }
            return "NASDAQ";
        }//getSecMarketName

        public static int getSecDatarowPosition(List<Entities.dbo.PortfolioSecurities> lstSelectedSecs, String strSecId)
        { // Retrieves the position of the current security
            for (int iSecs = 0; iSecs < lstSelectedSecs.Count; iSecs++)
                if (strSecId == lstSelectedSecs[iSecs].Securities.idSecurity)
                    return iSecs;
            return -1;
        }//getSecDatarowPosition

        #endregion Static methods

        #region Load Benchmarks from SQL Srv

        private cSecurity getCurrBMSecurity(BMsecurity drSec, IPortfolioBL cCurrPort)
        { // Retrieves an instance of a security based on its datarow info
            ICollectionsHandler cColHandler = cCurrPort.ColHandler;

            cSecurity cCurrSec = new cSecurity(cCurrPort, drSec.strName, drSec.strSymbol);  //111An
            cCurrSec.Properties.PortSecurityId = drSec.idSecurity;

            cCurrSec.Properties.HebName = drSec.strHebName;
            cCurrSec.FAC = 1D;

            cCurrSec.AvgYield = Convert.ToDouble(drSec.AvgYield);
            cCurrSec.StdYield = Convert.ToDouble(drSec.StdYield);

            cCurrSec.AvgYieldNIS = Convert.ToDouble(drSec.AvgYieldNIS);
            cCurrSec.StdYieldNIS = Convert.ToDouble(drSec.StdYieldNIS);

            //////cCurrSec.ValueUSA = Convert.ToDouble(drSec.dValueUSA);        // THEY ARE NULLS for BM sec
            //////cCurrSec.ValueNIS = Convert.ToDouble(drSec.dValueNIS);

            //////cCurrSec.WeightUSA = Convert.ToDouble(drSec.WeightUSA);
            //////cCurrSec.WeightNIS = Convert.ToDouble(drSec.WeightNIS);

            cCurrSec.DateRange = new cDateRange(DateTime.Today.AddYears(-cProperties.DatesInterval), DateTime.Today.AddDays(-1));
            lock (lockObject)
            {
                cCurrSec.Properties.Sector = cColHandler.getCatItemByID(enumCatType.Sector, Convert.ToInt32(drSec.idSector),
                    cColHandler.Sectors);
                // Exchange values
                cCurrSec.Properties.Market = cColHandler.getCatItemByID(enumCatType.StockMarket, drSec.idMarket, cColHandler.Markets);
                cCurrSec.Properties.MarketName = getSecMarketName(drSec.idMarket);
                
                cCurrSec.Properties.SecurityType = cColHandler.getCatItemByID(enumCatType.SecurityType,
                        drSec.idSecurityType, cColHandler.SecTypes);
            }
            //if (drSec["idCurrency"] != DBNull.Value) cCurrSec.IdCurrency = drSec["IdCurrency"].ToString();
            cCurrSec.IdCurrency = drSec.idCurrency; // WHY WAS IT HERE????? cProperties.CurrencyId;

            try
            { // Only exists in portfolio securities
                // For Benchmark we assign 'true'
                //if (drSec["isActiveSecurity"] != DBNull.Value) cCurrSec.setSecurityActivity(Convert.ToBoolean(drSec["isActiveSecurity"]));
                //else cCurrSec.setSecurityActivity(true);


                // LR: at this point security already has .PriceTable filled in, so commenting the line, because it crashes here
                //cCurrSec.PriceTable = drSec.Prices.ToList();

                //////foreach (var p in cCurrSec.PriceTable)
                //////{
                //////    p.dAdjPrice = p.fClose;
                //////}

                cCurrSec.setSecurityActivity(true);
            }
            catch (Exception ex)
            {

            }

            //// Made separate entity for tbl_IndexPrices
            //var priceRepository = Resolver.Resolve<IRepository>();
            //List<Entities.dbo.BMPrice> laura;
            //try
            //{
            //    priceRepository.Execute(session =>
            //    {
            //        ////cCurrSec.PriceTable = AutoMapper.Mapper.Map<List<Entities.dbo.Price>>(session.Query<Entities.dbo.BMPrice>().Where(x => x.idSecurity == "'" + drSec.idSecurity + "'").OrderByDescending(x => x.dDate).ToList());
            //        laura = session.Query<Entities.dbo.BMPrice>().Where(x => x.idSecurity == "'" + drSec.idSecurity + "'").OrderByDescending(x => x.dDate).ToList();
            //    });
            //}
            //catch (Exception ex)
            //{
            //    m_objErrorHandler.LogInfo(ex);
            //}
            //Resolver.Release(priceRepository);

            return cCurrSec;
        }//getCurrSecurity

        private cSecurity getCurrBMSecurity_with_EntitySec(Entities.dbo.Security drSec, IPortfolioBL cCurrPort)
        { // Retrieves an instance of a security based on its datarow info
            ICollectionsHandler cColHandler = cCurrPort.ColHandler;

            cSecurity cCurrSec = new cSecurity(cCurrPort, drSec.strName, drSec.strSymbol);  //111An
            cCurrSec.Properties.PortSecurityId = drSec.idSecurity;

            cCurrSec.Properties.HebName = drSec.strHebName;
            cCurrSec.FAC = 1D;

            ////if (drSec["FAC"] != DBNull.Value) cCurrSec.FAC = Convert.ToDouble(drSec["FAC"]);
            ////else cCurrSec.FAC = 1D;
            ////if (cCurrSec.FAC <= 0D) cCurrSec.FAC = 1D;

            if (drSec.AvgYield.HasValue) cCurrSec.AvgYield = Convert.ToDouble(drSec.AvgYield);
            if (drSec.StdYield.HasValue) cCurrSec.StdYield = Convert.ToDouble(drSec.StdYield);

            if (drSec.AvgYieldNIS.HasValue) cCurrSec.AvgYieldNIS = Convert.ToDouble(drSec.AvgYieldNIS);
            if (drSec.StdYieldNIS.HasValue) cCurrSec.StdYieldNIS = Convert.ToDouble(drSec.StdYieldNIS);

            if (drSec.MonetaryAvg.HasValue) cCurrSec.ValueUSA = Convert.ToDouble(drSec.MonetaryAvg);
            if (drSec.MonetaryAvgNIS.HasValue) cCurrSec.ValueNIS = Convert.ToDouble(drSec.MonetaryAvgNIS);

            if (drSec.WeightUSA.HasValue) cCurrSec.WeightUSA = Convert.ToDouble(drSec.WeightUSA);
            if (drSec.WeightNIS.HasValue) cCurrSec.WeightNIS = Convert.ToDouble(drSec.WeightNIS);

            //cCurrSec.Properties.ISIN = drSec.strISIN;

            ////if ((drSec["dtPriceStart"] != DBNull.Value) && (drSec["dtPriceEnd"] != DBNull.Value))
            ////    cCurrSec.DateRange = new cDateRange(Convert.ToDateTime(drSec["dtPriceStart"]), Convert.ToDateTime(drSec["dtPriceEnd"]));
            cCurrSec.DateRange = new cDateRange(DateTime.Today.AddYears(-cProperties.DatesInterval), DateTime.Today.AddDays(-1));

            if (drSec.idSector.HasValue) cCurrSec.Properties.Sector = cColHandler.getCatItemByID(enumCatType.Sector, Convert.ToInt32(drSec.idSector),
                    cColHandler.Sectors);
            if (drSec.idMarket.HasValue)
            { // Exchange values
                cCurrSec.Properties.Market = cColHandler.getCatItemByID(enumCatType.StockMarket, drSec.idMarket.Value, cColHandler.Markets);
                cCurrSec.Properties.MarketName = getSecMarketName(drSec.idMarket.Value);
            }
            if (drSec.idSecurityType.HasValue) cCurrSec.Properties.SecurityType = cColHandler.getCatItemByID(enumCatType.SecurityType,
                    drSec.idSecurityType.Value, cColHandler.SecTypes);

            //if (drSec["idCurrency"] != DBNull.Value) cCurrSec.IdCurrency = drSec["IdCurrency"].ToString();
            cCurrSec.IdCurrency = cProperties.CurrencyId;

            try
            { // Only exists in portfolio securities
                // For Benchmark we assign 'true'
                //if (drSec["isActiveSecurity"] != DBNull.Value) cCurrSec.setSecurityActivity(Convert.ToBoolean(drSec["isActiveSecurity"]));
                //else cCurrSec.setSecurityActivity(true);




                // LR: at this point security already has .PriceTable filled in, so commenting the line, because it crashes here
                //cCurrSec.PriceTable = drSec.Prices.ToList();

                //////foreach (var p in cCurrSec.PriceTable)
                //////{
                //////    p.dAdjPrice = p.fClose;
                //////}

                cCurrSec.setSecurityActivity(true);
            }
            catch (Exception ex)
            {

            }

            return cCurrSec;
        }//getCurrSecurity

        private void loadBenchmarksFromDB()
        { // Loads Benchmarks from SQL Srv

            m_colBenchmarks = StaticData<cSecurity, ISecurities>.BenchMarks;







            // TODO: WHY ARE WE DOING THIS   ?????????????????????

            // Reverse Bench rates (ONLY IF  ORIGINAL 'PriceTable' IS IN ASCENDING ORDER)
            // Check why v_Prices in DB doesn't have order by?? (probably because it will take a lot of time. So where is 'PriceTable' sorted???)
            for (int iSecs = 0; iSecs < m_colBenchmarks.Count; iSecs++)
            {
                if (m_colBenchmarks[iSecs].PriceTable.Count > 1 && m_colBenchmarks[iSecs].PriceTable[1].dDate > m_colBenchmarks[iSecs].PriceTable[0].dDate)
                {
                    List<Entities.dbo.Price> colTempPrices = new List<Entities.dbo.Price>();
                    for (int iRows = m_colBenchmarks[iSecs].PriceTable.Count - 1; iRows >= 0; iRows--)
                        colTempPrices.Add(m_colBenchmarks[iSecs].PriceTable[iRows]);
                    m_colBenchmarks[iSecs].PriceTable = colTempPrices;
                }
            }
        }//loadBenchmarksFromDB

        //private bool GetBenchmarkPricesDataTableInDaterange(string SecurityCode, out DataSet dsMain)
        //{// Read Benchmark Prices in Date range with interval

        //    String BenchmarksIDlist = "137, 143, 142, 709, 0.5177, 0.2020, 0.1297, 2, 147, 168, 164";

        //    m_enumErrorCode = ClientBiz.cltBiz.GetHistoricalDataForBenchmarks(BenchmarksIDlist, DateTime.Today.AddYears(-cProperties.DatesInterval), DateTime.Today.AddDays(-1), SecurityCode, out dsMain, out m_strErrorMsg);

        //    if (m_enumErrorCode != TFI_CS_Shared.ResultCodes.ok)
        //    {
        //        m_objErrorHandler.LogInfo(new Exception("m_strErrorMsg"));
        //        //m_frmMain.StatusLine.writeMessage("Error: " + m_strErrorMsg);
        //        return false;
        //    }

        //    // Check for 2 tables
        //    if (dsMain.Tables.Count == 2)
        //        return true;
        //    else return false;
        //}//GetPricesDataTablesInDaterange

        ////public void setPortfolioPricesFromDB()
        ////{ // Loads the prices relevant to the current portfolio
        ////    try
        ////    {
        ////        for (int iSecs = 0; iSecs < Benchmarks.Count; iSecs++)  // Loads all securities
        ////            Benchmarks[iSecs].PriceTable = cDbOleConnection.FillDataTable(cSqlStatements.getSecurityPrices(new cDateRange(DateTime.Today.AddYears(-cProperties.DatesInterval), DateTime.Today.AddDays(-1)),
        ////                                                           Benchmarks[iSecs].Properties.PortSecurityId), m_objOleDBConn.dbConnection);
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        m_objErrorHandler.LogInfo(ex);
        ////    }
        ////}//setPortfolioPrices



        #endregion

        #endregion Methods

        #region Properties

        public ISecurities FullSecurities
        { get { return m_colFullCollection; } }//FullSecurities

        public ISecurities Securities
        {
            get { return m_colSecurities; }
            set { m_colSecurities = value; }
        }//Securities

        public ISecurities SecuritiesByRisk
        {
            get { return m_colSecuritiesByRisk; }
            set { m_colSecuritiesByRisk = value; }
        }//SecuritiesByRisk

        public ISecurities ActiveSecs
        {
            get { return m_colActiveSecurities; }
            set { m_colActiveSecurities = value; }
        }//ActiveSecurities

        public ISecurities Benchmarks
        {
            get { return m_colBenchmarks; }
            set { m_colBenchmarks = value; }
        }//Benchmarks

        public ISecurities DisabledSecs
        {
            get { return m_colDisabledSecurities; }
            set { m_colDisabledSecurities = value; }
        }//DisabledSecs

        public ICategoryCollection Sectors
        {
            get { return m_colSectors; }
            set { m_colSectors = value; }
        }//Sectors

        public ICategoryCollection Markets
        {
            get { return m_colMarkets; }
            set { m_colMarkets = value; }
        }//Markets

        public ICategoryCollection SecTypes
        {
            get { return m_colSecTypes; }
            set { m_colSecTypes = value; }
        }//SecTypes

        public Boolean isModifiedSelection
        {
            get { return m_isModifiedSelection; }
            set { m_isModifiedSelection = value; }
        }//isModifiedSelection

        public Boolean isPortfolioSec
        { get { return m_isPortSec; } }//isPortfolioSec
        
        #endregion Properties

    }//of class
}
