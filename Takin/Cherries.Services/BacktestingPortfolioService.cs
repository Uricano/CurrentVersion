using Cherries.Models.App;
using Cherries.Models.Command;
using Cherries.Models.dbo;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using Cherries.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFI.BusinessLogic.Interfaces;

namespace Cherries.Services
{
    public class BacktestingPortfolioService : IBacktestingPortfolioService, IServiceBase
    {

        private IManageBacktestingPortfolios cPortHandler;// = new cManagePortfolios(new cPortfolio(new cErrorHandler()));
        private IOptimizationService opService;
        public BacktestingPortfolioService(IManageBacktestingPortfolios managePort, IOptimizationService op)
        {
            cPortHandler = managePort;
            opService = op;
        }

        //public BacktestingPortfolioViewModel GetPortolios(long userId)
        //{
        //    List<PortfolioDetails> res = new List<PortfolioDetails>();
        //    res = cPortHandler.GetPortfolioDetailsList(userId);
        //    BacktestingPortfolioViewModel vm = new BacktestingPortfolioViewModel();
        //    vm.Details = res;
        //    return vm;
        //}//GetPortolios

        public BacktestingPortsViewModel GetPortolios(GetPortfolioQuery query)
        {
            TopPortfoliosViewModel ports = new TopPortfoliosViewModel();
            ports = cPortHandler.GetPortfolioDetailsList(query);
            List<BacktestingPort> res = new List<BacktestingPort>();
            for (int iPorts = 0; iPorts < ports.Portfolios.Count; iPorts++)
            {
                BacktestingPort newPort = new BacktestingPort();
                newPort.Details = ports.Portfolios[iPorts];
                newPort.StartDate = newPort.Details.StartDate;
                newPort.EndDate = newPort.Details.EndDate;
                newPort.BenchmarkNames = cPortHandler.getPortfolioBenchmarkNames(newPort.Details.ID);

                res.Add(newPort);
            }

            BacktestingPortsViewModel vm = new BacktestingPortsViewModel();
            vm.Ports = res;
            vm.NumOfRecords = ports.NumOfRecords;
            return vm;
        }//GetPortolios

        public BacktestingPortfolioViewModel GetFullPortfolio(User user, int id)
        {
            BacktestingPortfolioViewModel vm = new BacktestingPortfolioViewModel();
            vm = cPortHandler.GetFullPortfolio(id, user.Currency.CurrencyId, user.Licence.Stocks.Select(t => t.id).ToList());
            return vm;
        }//GetFullPortfolio

        public BaseViewModel IsMaxPortfolioExceeded(User user)
        {
            BaseViewModel vm = new BaseViewModel();
            var count = cPortHandler.GetPortfolioCount(user.UserID);
            if (count >= user.Licence.Service.Iportfolios)
                vm.Messages.Add(new Message { LogLevel = LogLevel.Error, Text = "You've exceeded the number of portfolios you can create. Please delete any unwanted portfolios" });
            return vm;
        }//IsMaxPortfolioExceeded

        public BaseViewModel IsPortfolioExists(string name, long userId)
        {
            BaseViewModel vm = new BaseViewModel();
            var exists = cPortHandler.IsPortfolioExists(name, userId);
            if (exists)
                vm.Messages.Add(new Message { LogLevel = LogLevel.Error, Text = $"Portfolio named {name} already exists" });
            return vm;
        }//IsPortfolioExists

        public BaseViewModel DeletePortfolio(User user, int portId)
        {
            var vm = cPortHandler.DeletePortfolio(portId);
            return vm;
        }
    }//of class
}
