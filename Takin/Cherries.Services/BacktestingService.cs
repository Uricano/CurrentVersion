using Cherries.Models.App;
using Cherries.Models.Command;
using Cherries.Models.dbo;
using Cherries.Models.Lookup;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using Cherries.Services.Interfaces;
using Cherries.TFI.BusinessLogic.General;
//using Cherries.TFI.BusinessLogic.Optimization.Backtesting;
using Cherries.TFI.BusinessLogic.Portfolio;
using Cherries.TFI.BusinessLogic.Securities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFI.BusinessLogic.Interfaces;

namespace Cherries.Services
{
    public class BackTestingService : IBackTestingService, IServiceBase
    {
        private IPortfolioBL cPort;
        IBacktestingCalculation backTesingCalc;
        IManageBacktestingPortfolios _manageBacktestingPortfolios;
        private IBacktestingHandler _backtestingHandler;
        public BackTestingService(IPortfolioBL portBL, IManageBacktestingPortfolios manageBacktestingPortfolios, IBacktestingHandler backtestingHandler)
        {
            cPort = portBL;
            //backTesingCalc = backTesing;
            _manageBacktestingPortfolios = manageBacktestingPortfolios;
            _backtestingHandler = backtestingHandler;
        }
        public BackTestingViewModel GetPortfolioBackTesting(User user,BackTestingQuery query)
        {
            BackTestingViewModel vm = new BackTestingViewModel();
            //cPort.openExistingPortfolio(query.PortID);
            //if (!cPort.instantiatePortfolioVariables(true, cPort.Details.CalcCurrency, user.Licence.Stocks.Select(t=>t.id).ToList(), cPort.Details.SecurityData.Select(x=>x.idSecurity).ToList()))
            //    vm.Messages.Add(new Models.App.Message { LogLevel = Models.App.LogLevel.Error, Text = "Error Get Portfolio Data. Please Contanct System Administrator" });
            //else
            //    vm = backTesingCalc.calculateBacktesting(cPort, query.StartDate, query.EndDate, query.Equity, query.BenchMarkID);
            return vm;
        }

        public BaseViewModel IsPortfolioExists(string name, long userId)
        {
            BaseViewModel vm = new BaseViewModel();
            var exists = _manageBacktestingPortfolios.IsPortfolioExists(name, userId);
            if (exists)
                vm.Messages.Add(new Message { LogLevel = LogLevel.Error, Text = $"Backtesting portfolio named {name} already exists" });
            return vm;
        }

        public BackTestingViewModel calculateBacktesting(User user, Models.Command.CreatePortfolioCommand cmd, DateTime dtStartDate, DateTime dtEndDate, List<string> becnhMarkIDs)
        { // Service which creates a new backtesting portfolio + calculates proper backtesting values 
            BackTestingViewModel vmBacktesting = new BackTestingViewModel();

            // Set up portfolio
            int idPortfolio = _manageBacktestingPortfolios.CreateDefaultPortfolio(user, dtStartDate, dtEndDate, cmd);
            _manageBacktestingPortfolios.SelectedPortfolio.openExistingPortfolio(idPortfolio, true);
            _backtestingHandler.setBacktestingPortfolio(_manageBacktestingPortfolios.SelectedPortfolio, dtStartDate, dtEndDate, cmd.Equity);
            cPort = _manageBacktestingPortfolios.SelectedPortfolio;

            // Set portfolio data
            List<int> colExchanges = (cmd.Exchanges != null && cmd.Exchanges.Count > 0 && cmd.Exchanges.Count <= user.Licence.Stocks.Count) ? cmd.Exchanges : user.Licence.Stocks.Select(t => t.id).ToList();
            if (!_manageBacktestingPortfolios.SelectedPortfolio.instantiateVariablesForPortfolio(true, cPort.Details.CalcCurrency, colExchanges, cmd.Securities))
            { // Failed to get data
                vmBacktesting.Messages.Add(new Models.App.Message { LogLevel = Models.App.LogLevel.Error, Text = "Error Get Portfolio Data. Please Contanct System Administrator" });
                return vmBacktesting;
            }

            if(cmd.Securities.Count > 0)
                _manageBacktestingPortfolios.SelectedPortfolio.ColHandler.ActiveSecs = getFilteredSecByCustomList(cmd);
            else
                _manageBacktestingPortfolios.SelectedPortfolio.ColHandler.ActiveSecs = getFilteredSecByExchangesAndRisk(colExchanges);


            // Run calculation
            //vmBacktesting = _backtestingHandler.calculateBacktesting(_manageBacktestingPortfolios.SelectedPortfolio, dtStartDate, dtEndDate, cmd.Equity, becnhMarkIDs);
            vmBacktesting = _backtestingHandler.calculateNewBacktesting(_manageBacktestingPortfolios.SelectedPortfolio, dtStartDate, dtEndDate, cmd.Equity, becnhMarkIDs);

            // Save portfolio data
            // Create command to update portfolio + portfolio securities
            UpdatePortfolioCommand pc = new UpdatePortfolioCommand();
            pc.CalcType = cmd.CalcType;
            pc.Equity = cmd.Equity;
            pc.PortID = _manageBacktestingPortfolios.SelectedPortfolio.Details.ID;
            pc.Risk = vmBacktesting.Portfolios[vmBacktesting.PortNumA].Risk;
            pc.Securities = vmBacktesting.Portfolios[vmBacktesting.PortNumA].Securities;

            _manageBacktestingPortfolios.UpdatePortfolio(pc, vmBacktesting);
            return vmBacktesting;
        }//calculateBacktesting

        public double getBenchmarkRisk(int iBenchPos)
        {
            return _backtestingHandler.Benchmarks[iBenchPos].CovarClass.StandardDeviation;
        }//getBenchmarkRisk


        private ISecurities getFilteredSecByExchangesAndRisk(List<int> colExchanges)
        {
            ISecurities newColl = new cSecurities(cPort);
            for (int i = 0; i < _manageBacktestingPortfolios.SelectedPortfolio.ColHandler.Securities.Count; i++)
            {
                if (colExchanges.Contains(_manageBacktestingPortfolios.SelectedPortfolio.ColHandler.Securities[i].Properties.Market.ID))
                    newColl.Add(_manageBacktestingPortfolios.SelectedPortfolio.ColHandler.Securities[i]);
            }

            return newColl;
        }

        private ISecurities getFilteredSecByCustomList(Models.Command.CreatePortfolioCommand cmd)
        {
            List<string> missingSecs = new List<string>();

            ISecurities newColl = new cSecurities(cPort);

            for (int i = 0; i < cmd.Securities.Count; i++)
            {
                var sec = cPort.ColHandler.Securities.getSecurityByIdNormalSearch(cmd.Securities[i]);
                if (sec != null)
                    newColl.Add(sec);
                else
                    missingSecs.Add(cmd.Securities[i]);
            }

            if (missingSecs.Count > 0)
            {
                cPort.ColHandler.addMissingSecurities(newColl, missingSecs);
            }
            //********************************************************
            //IRepository repository;
            //repository = Resolver.Resolve<IRepository>();
            //TopSecuritiesViewModel vm = new TopSecuritiesViewModel();
            //repository.Execute(session =>
            //{
            //    var securities = session.Query<TopSecurities>().Where(x => query.Securities.Contains(x.idSecurity));

            //    //return AutoMapper.Mapper.Map<List<Models.dbo.Security>>(topSecs);

            //    //TODO: have to do mapping probably
            //    newColl = newColl.GetCustomSecurities(securities);
            //});
            //********************************************************
            return newColl;
        }


        public BacktestingPortfolioViewModel GetPortolios(long userId)
        {
            List<PortfolioDetails> res = new List<PortfolioDetails>();
            res = _manageBacktestingPortfolios.GetPortfolioDetailsList(userId);
            BacktestingPortfolioViewModel vm = new BacktestingPortfolioViewModel();
            vm.Details = res;
            return vm;
        }

        public BacktestingPortfolioViewModel GetFullPortfolio(User user, int id)
        {
            BacktestingPortfolioViewModel vm = new BacktestingPortfolioViewModel();
            vm = _manageBacktestingPortfolios.GetFullPortfolio(id, user.Currency.CurrencyId, user.Licence.Stocks.Select(t => t.id).ToList());

            //_backtestingHandler.setBacktestingPortfolio(_manageBacktestingPortfolios.SelectedPortfolio, _manageBacktestingPortfolios.SelectedPortfolio.Details.DateEdited, _manageBacktestingPortfolios.SelectedPortfolio.Details.LastOptimization, _manageBacktestingPortfolios.SelectedPortfolio.Details.Equity);
            

            return vm;
        }

        public BackTestingViewModel GetBacktestingPortfolio(User user, int id)
        {
            // BacktestingPortfolioViewModel vmP = new BacktestingPortfolioViewModel();
            // vmP = _manageBacktestingPortfolios.GetFullPortfolio(id, user.Currency.CurrencyId, user.Licence.Stocks.Select(t => t.id).ToList());
            BackTestingViewModel vmB = new BackTestingViewModel();
            var stocks =  user.Licence.Stocks.Select(t => t.id).ToList();
            _manageBacktestingPortfolios.SelectedPortfolio.openExistingPortfolio(id, true);

            cPort = _manageBacktestingPortfolios.SelectedPortfolio;

            //if (!_manageBacktestingPortfolios.SelectedPortfolio.instantiatePortfolioVariables(true, cPort.Details.CalcCurrency, stocks, null))
            if (!_manageBacktestingPortfolios.SelectedPortfolio.instantiateVariablesForPortfolio(false, cPort.Details.CalcCurrency, stocks, null))
            {
                vmB.Messages.Add(new Models.App.Message { LogLevel = Models.App.LogLevel.Error, Text = "Error Get Portfolio Data. Please Contanct System Administrator" });
                return vmB;
            }

            
            //_backtestingHandler.setBacktestingPortfolio(_manageBacktestingPortfolios.SelectedPortfolio, _manageBacktestingPortfolios.SelectedPortfolio.Details.DateEdited, _manageBacktestingPortfolios.SelectedPortfolio.Details.LastOptimization, _manageBacktestingPortfolios.SelectedPortfolio.Details.Equity);
            //vmB = _backtestingHandler.getBacktestingPortfolio(_manageBacktestingPortfolios.SelectedPortfolio, _manageBacktestingPortfolios.SelectedPortfolio.Details.DateEdited, _manageBacktestingPortfolios.SelectedPortfolio.Details.LastOptimization, _manageBacktestingPortfolios.SelectedPortfolio.Details.Equity);
            _backtestingHandler.setBacktestingPortfolio(_manageBacktestingPortfolios.SelectedPortfolio, (DateTime)_manageBacktestingPortfolios.SelectedPortfolio.Details.StartDate, (DateTime)_manageBacktestingPortfolios.SelectedPortfolio.Details.EndDate, _manageBacktestingPortfolios.SelectedPortfolio.Details.Equity);
            //vmB = _backtestingHandler.getBacktestingPortfolio(_manageBacktestingPortfolios.SelectedPortfolio, (DateTime)_manageBacktestingPortfolios.SelectedPortfolio.Details.StartDate, (DateTime)_manageBacktestingPortfolios.SelectedPortfolio.Details.EndDate, _manageBacktestingPortfolios.SelectedPortfolio.Details.Equity);
            vmB = _backtestingHandler.getNewBacktestingPortfolio(_manageBacktestingPortfolios.SelectedPortfolio, (DateTime)_manageBacktestingPortfolios.SelectedPortfolio.Details.StartDate, (DateTime)_manageBacktestingPortfolios.SelectedPortfolio.Details.EndDate, _manageBacktestingPortfolios.SelectedPortfolio.Details.Equity);

            return vmB;
        }//GetBacktestingPortfolio

    }
}
