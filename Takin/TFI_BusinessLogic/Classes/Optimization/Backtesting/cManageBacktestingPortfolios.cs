using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Used namespaces
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.DataManagement;
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.Protection.LicenseManagement;
using Ness.DataAccess.Repository;
using System.Linq;
using Cherries.Models.App;
using Cherries.Models.ViewModel;
using TFI.Entities.Sp;
using Cherries.Models.dbo;
using Cherries.Models.Command;
using Cherries.TFI.BusinessLogic.StaticMethods;
using NHibernate.Linq;
using TFI.BusinessLogic.Interfaces;
using TFI.BusinessLogic.Bootstraper;
using Entities = TFI.Entities;
using Cherries.Models.Queries;

namespace TFI.BusinessLogic.Classes.Optimization.Backtesting
{
    public class cManageBacktestingPortfolios : IManageBacktestingPortfolios
    {

        #region Data Members

        // Main variables
        private IPortfolioBL m_objPortfolio;     // Portfolio class pointer

        private IErrorHandler m_objErrorHandler; // Error handler class
        private ISecurities securitiesBL;
        // Data variables
        private DataTable m_dtPortfolios;       // Portfolios Table
        private int m_iPortNum = 0;             // Number of portfolios (in DB)
        private int m_iRealPortNum = 0;         // Number of real portfolios (in DB)
        private IRepository repository;// = new Repository();
        private bool _isBacktestingPort = true;      // not backtesting portfolio

        #endregion Data Members

        #region Consturctors, Initialization & Destructor

        public cManageBacktestingPortfolios(IPortfolioBL cPort, IRepository rep)
        {
            m_objPortfolio = cPort;
            m_objErrorHandler = m_objPortfolio.cErrorLog;
            repository = rep;
            //m_objDbConnection = m_objPortfolio.OleDBConn;
            securitiesBL = new cSecurities(cPort);
            try
            {
                //setPortfolioTable();
            }
            catch (Exception ex)
            {
                m_objErrorHandler.LogInfo(ex);
            }
        }//constructor

        #endregion Consturctors, Initialization & Destructor

        #region Methods

        #region Open portfolio

        public TopPortfoliosViewModel GetPortfolioDetailsList(GetPortfolioQuery query)
        {
            TopPortfoliosViewModel vm = new TopPortfoliosViewModel();
            var parameters = new Dictionary<string, Tuple<object, NHibernate.Type.IType>>();
            parameters.Add("UserId", new Tuple<object, NHibernate.Type.IType>(query.userId, NHibernate.NHibernateUtil.Int64));
            int startRow = (query.pageNumber - 1) * query.pageSize;
            repository.Execute(session =>
            {
                vm.NumOfRecords = session.Query<Entities.dbo.BacktestingPortfolio>().Where(x => x.userId == query.userId && x.iSecsNum > 0).Count();
            });
            
            parameters.Add("offset", new Tuple<object, NHibernate.Type.IType>(startRow, NHibernate.NHibernateUtil.Int32));
            parameters.Add("count", new Tuple<object, NHibernate.Type.IType>(query.pageSize, NHibernate.NHibernateUtil.Int32));
            parameters.Add("sortField", new Tuple<object, NHibernate.Type.IType>(query.sortField, NHibernate.NHibernateUtil.String));
            var portfolios = repository.ExecuteSp<UserPortfolio>("dataGetUserBacktestingPortfolio", parameters);
            vm.Portfolios = AutoMapper.Mapper.Map<List<PortfolioDetails>>(portfolios);

            repository.Execute(session =>
            {
                foreach (var item in vm.Portfolios)
                {
                    item.PreferedRisk = m_objPortfolio.GetRisk(item.InitRisk);
                }
            });


            return vm;
        }

        public List<PortfolioDetails> GetPortfolioDetailsList(long userId)
        {
            List<PortfolioDetails> res = new List<PortfolioDetails>();
            var parameters = new Dictionary<string, Tuple<object, NHibernate.Type.IType>>();
            parameters.Add("UserId", new Tuple<object, NHibernate.Type.IType>(userId, NHibernate.NHibernateUtil.Int64));
            parameters.Add("offset", new Tuple<object, NHibernate.Type.IType>(0, NHibernate.NHibernateUtil.Int32));
            parameters.Add("count", new Tuple<object, NHibernate.Type.IType>(5, NHibernate.NHibernateUtil.Int32));
            parameters.Add("sortField", new Tuple<object, NHibernate.Type.IType>("DateCreated", NHibernate.NHibernateUtil.String));
            var portfolios = repository.ExecuteSp<UserPortfolio>("dataGetUserBacktestingPortfolio", parameters);
            res = AutoMapper.Mapper.Map<List<PortfolioDetails>>(portfolios);
            repository.Execute(session =>
            {
                foreach (var item in res)
                {
                    item.PreferedRisk = m_objPortfolio.GetRisk(item.InitRisk);
                }
            });


            return res;
        }
        public List<String> getPortfolioBenchmarkNames(long portID)
        { // Retrieves a list of benchmarks for a given portfolio
            List<String> colBenchmarks = new List<string>();
            repository.Execute(session =>
            {
                var portfolios = session.Query<Entities.dbo.BacktestingPortfolioSecurities>().Where(x => x.Portfolios.idPortfolio == portID);

                foreach (Entities.dbo.BacktestingPortfolioSecurities sec in portfolios)
                    if (sec.isBenchmark == true && sec.Securities != null)
                    {
                        colBenchmarks.Add(StaticData<cSecurity, ISecurities>.BenchMarks.getSecurityByIdNormalSearch(sec.Securities.idSecurity).Properties.SecurityName);
                        // LR: replaced with the line above, because we don't have Entity.dbo.Securities filled in (had propblems loading securities with entities, so did it with SP)
                        //colBenchmarks.Add(sec.Securities.strName);
                    }
            });

            return colBenchmarks;
        }//getPortfolioBenchmarkNames

        public BacktestingPortfolioViewModel GetFullPortfolio(int id, string currency, List<int> exchangesPackagees)
        {
            this.openSelectedPortfolio(id, _isBacktestingPort);
            m_objPortfolio.instantiateVariablesForPortfolio(false, m_objPortfolio.Details.CalcCurrency, exchangesPackagees);
            BacktestingPortfolioViewModel vm = new BacktestingPortfolioViewModel();
            vm.Details = new List<PortfolioDetails>() { m_objPortfolio.Details };
            return vm;
        }

        public int GetPortfolioCount(long userID)
        {
            int count = 0;
            repository.Execute(session =>
            {
                count = session.Query<Entities.dbo.BacktestingPortfolio>().Where(x => x.userId == userID).Count();
            });
            return count;
        }

        public bool IsPortfolioExists(string name, long userId)
        {
            bool exists = false;
            repository.Execute(session =>
            {
                exists = session.Query<Entities.dbo.BacktestingPortfolio>().Where(x => x.strName == name && x.userId == userId).Count() > 0;
            });
            return exists;
        }

        public void openSelectedPortfolio(int iPortID, bool isBacktPort)
        { // Opens selected portfolio
            try
            {
                m_objPortfolio.openExistingPortfolio(iPortID, isBacktPort);
            }
            catch (Exception ex)
            {
                m_objErrorHandler.LogInfo(ex);
            }
        }//openSelectedPortfolio


        #endregion Open portfolio

        #region Create portfolio

        //public IPortfolioBL setNewBacktestingPortfolio(int idUser, )

        public int CreateDefaultPortfolio(User user, DateTime dtStartDate, DateTime dtEndDate, CreatePortfolioCommand cmd)
        { // Creates a new backtesting portfolio
            m_objPortfolio.Details = new PortfolioDetails();
            m_objPortfolio.Details.Code = getRandomPortfolioCode();
            m_objPortfolio.Details.Name = cmd.Name;
            m_objPortfolio.Details.CurrEquity = (float)cmd.Equity;
            m_objPortfolio.Details.PreferedRisk = m_objPortfolio.GetRisk(cmd.Risk);
            //m_objPortfolio.Details.CalcCurrency = user.Currency.CurrencyId; // WHY is it commented? Causes problems in BacktestingService, f. calculateBackteting. 
            m_objPortfolio.Details.DateCreated = DateTime.Today;
            m_objPortfolio.Details.DateEdited = DateTime.Today;
            m_objPortfolio.Details.StartDate = dtStartDate;
            m_objPortfolio.Details.EndDate = dtEndDate;
            m_objPortfolio.Details.Equity = (float)cmd.Equity;
            //m_objPortfolio.Details.MaxSecs = user.Licence.Service.SecsPerPort;
            m_objPortfolio.Details.CalcType = cmd.CalcType;
            m_objPortfolio.Details.LastOptimization = DateTime.Today;
            m_objPortfolio.Details.UserID = user.UserID;
            m_objPortfolio.Details.CurrentStDev = cmd.Risk;
            m_objPortfolio.Details.isManual = cmd.Securities.Count > 0;
            var port = AutoMapper.Mapper.Map<Entities.dbo.BacktestingPortfolio>(m_objPortfolio.Details);
            m_objPortfolio.Details.ID = port.idPortfolio;

            //LR on 13_09_18
            cmd.Exchanges = cmd.Exchanges != null && cmd.Exchanges.Count > 0
                                && cmd.Exchanges.Count <= user.Licence.Stocks.Count ? cmd.Exchanges : user.Licence.Stocks.Select(t => t.id).ToList();

            repository.Execute(session =>
            {
                port.CalcCurrency = user.Licence.Stocks.Where(x => cmd.Exchanges.Contains(x.id)).OrderBy(s => s.CurrencyRank).FirstOrDefault().Currency;
                session.SaveOrUpdate(port);
            });
            m_objPortfolio.Details.CalcCurrency = port.CalcCurrency;

            return port.idPortfolio;
        }//CreateDefaultPortfolio

        public BaseViewModel UpdatePortfolio(UpdatePortfolioCommand cmd, BackTestingViewModel vmBack)
        {
            BaseViewModel vm = new BaseViewModel();
            Entities.dbo.BacktestingPortfolio portEntity = null; ;
            repository.Execute(session =>
            {
                portEntity = session.Get<Entities.dbo.BacktestingPortfolio>(cmd.PortID);
                AutoMapper.Mapper.Map<BasePortfolioCommand, Entities.dbo.BacktestingPortfolio>(cmd, portEntity);
                portEntity.dCurrRisk = cmd.Risk;
                portEntity.iSecsNum = cmd.Securities.Count;
                portEntity.dCurrEquity = vmBack.Equity;
            });

            var secList = AutoMapper.Mapper.Map<List<Entities.dbo.BacktestingPortfolioSecurities>>(cmd.Securities).Where(x => x.Securities.idSecurity != "0000").ToList();
            repository.ExecuteTransaction(session =>
            {
                session.Update(portEntity);
            });
            DeletePortfolioSecurities(cmd.PortID);
            repository.ExecuteTransactionStateLess(session =>
            {
                foreach (var item in secList)
                {
                    try
                    {
                        item.idPortSec = Guid.NewGuid();
                        item.Portfolios = new Entities.dbo.BacktestingPortfolio { idPortfolio = cmd.PortID };

                        session.Insert(item);
                    }
                    catch (Exception e)
                    {
                        m_objErrorHandler.LogInfo(e);
                    }
                }
            }, secList.Count);
            return vm;
        }

        public BaseViewModel DeletePortfolio(int PortID)
        {
            BaseViewModel vm = new BaseViewModel();
            DeletePortfolioSecurities(PortID);
            repository.ExecuteTransaction(session =>
            {
                var port = session.Get<Entities.dbo.BacktestingPortfolio>(PortID);
                session.Delete(port);
            });
            return vm;
        }

        private void DeletePortfolioSecurities(int PortID)
        {
            Dictionary<string, Tuple<object, NHibernate.Type.IType>> param = new Dictionary<string, Tuple<object, NHibernate.Type.IType>>();
            param.Add("PortID", new Tuple<object, NHibernate.Type.IType>(PortID, NHibernate.NHibernateUtil.Int32));
            repository.ExecuteSp("DeleteBacktestingPortfolioSecurities", param);
        }

        private String getRandomPortfolioCode()
        { // Retrieves portfolio code
            Random rnd = new Random();
            return "CH" + rnd.Next(1000, 9999).ToString() + "TAK" + rnd.Next(100, 999).ToString();
        }//getRandomPortfolioCode

        #endregion Create portfolio

        #endregion Methods

        #region Properties

        public IPortfolioBL SelectedPortfolio
        { get { return m_objPortfolio; } }//SelectedPortfolio

        public int NumOfPortfolios
        { get { return m_iPortNum; } }//NumOfPortfolios

        public int NumOfRealPortfolios
        { get { return m_iRealPortNum; } }//NumOfRealPortfolios

        public DataTable Portfolios
        { get { return m_dtPortfolios; } }//Portfolios


        #endregion Properties

    }//of class
}
