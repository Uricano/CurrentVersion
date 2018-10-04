using Cherries.Models.App;
using Cherries.Models.Command;
using Cherries.Models.dbo;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using Cherries.Services.Interfaces;
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Portfolio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFI.BusinessLogic.Enums;
using TFI.BusinessLogic.Interfaces;

namespace Cherries.Services
{
    public class PortfolioService : IPortfolioService
    {
        private IManagePortfolios cPortHandler;// = new cManagePortfolios(new cPortfolio(new cErrorHandler()));
        private IOptimizationService opService;
        public PortfolioService(IManagePortfolios managePort, IOptimizationService op)
        {
            cPortHandler = managePort;
            opService = op;
        }

        public PortfolioViewModel GetPortolios(long userId)
        {
            List<PortfolioDetails> res = new List<PortfolioDetails>();
            res = cPortHandler.GetPortfolioDetailsList(userId);
            PortfolioViewModel vm = new PortfolioViewModel();
            vm.Details = res;
            return vm;
        }

        public TopPortfoliosViewModel GetPortolios(GetPortfolioQuery query)
        {
            TopPortfoliosViewModel vm = new TopPortfoliosViewModel();
            vm =  cPortHandler.GetPortfolioDetailsList(query);
            return vm;
        }

        public PortfolioViewModel GetFullPortfolio(User user, int id)
        {
            PortfolioViewModel vm = new PortfolioViewModel();
            //vm = cPortHandler.GetFullPortfolio(id, user.Currency.CurrencyId, user.Licence.Stocks.Select(t => t.id).ToList());
            vm = cPortHandler.openPortfolio(id, user.Currency.CurrencyId, user.Licence.Stocks.Select(t => t.id).ToList());
            return vm;
        }//GetFullPortfolio

        public BaseViewModel IsMaxPortfolioExceeded(User user)
        {
            BaseViewModel vm = new BaseViewModel();
            var count = cPortHandler.GetPortfolioCount(user.UserID);
            if (count >= user.Licence.Service.Iportfolios)
                vm.Messages.Add(new Message { LogLevel = LogLevel.Error, Text = "You've exceeded the number of portfolios you can create. Please delete any unwanted portfolios" });
            return vm;
        }

        public BaseViewModel IsPortfolioExists(string name, long userId)
        {
            BaseViewModel vm = new BaseViewModel();
            var exists = cPortHandler.IsPortfolioExists(name, userId);
            if (exists)
                vm.Messages.Add(new Message { LogLevel = LogLevel.Error, Text = $"Portfolio named {name} already exists" });
            return vm;
        }

        public OptimalPortoliosViewModel CreatePortfolio(User user, CreatePortfolioCommand cmd)
        {
            if (cmd.CalcType == enumEfCalculationType.Custom && cmd.Exchanges.Count < 1)
            {
                for (int i = 0; i < user.Licence.Stocks.Count; i++)
                {
                    cmd.Exchanges.Add(Convert.ToInt32(user.Licence.Stocks[i].id));
                }
            }

            OptimalPortoliosViewModel vm = new OptimalPortoliosViewModel();
            var portID = 0;
            var count = cPortHandler.GetPortfolioCount(user.UserID);
            if (count < user.Licence.Service.Iportfolios)
                portID = cPortHandler.CreateDefaultPortfolio(user, cmd);
            else
                vm.Messages.Add(new Message { LogLevel = LogLevel.Error, Text = "You've exceeded the number of portfolios you can create. Please delete any unwanted portfolios" });
            if (portID > 0)
            {
                vm = opService.GetPortfolioOptimazation(user, new Models.Queries.OptimazationQuery
                {
                    CalcType = cmd.CalcType,
                    PortID = portID,
                    Securities = cmd.Securities,
                    Exchanges = cmd.Exchanges,
                    Risk = cmd.Risk //LR on 18/07/18
                });
                if (vm.Messages.Count == 0)
                {
                    cPortHandler.SelectedPortfolio.Details.LastOptimization = DateTime.Today;
                    cPortHandler.SelectedPortfolio.Details.SecsNum = vm.Portfolios[vm.PortNumA].Securities.Count;
                    cPortHandler.SelectedPortfolio.Details.CurrentStDev = vm.Portfolios[vm.PortNumA].Risk;
                    UpdatePortfolioCommand updCmd = new UpdatePortfolioCommand
                    {
                        CalcType = cPortHandler.SelectedPortfolio.Details.CalcType,
                        Equity = cmd.Equity,
                        Risk = vm.Portfolios[vm.PortNumA].Risk,
                        PortID = portID,
                        Securities = vm.Portfolios[vm.PortNumA].Securities
                    };
                    cPortHandler.UpdatePortfolio(updCmd);
                }
                else
                {
                    cPortHandler.DeletePortfolio(portID);
                }
            }

            return vm;
        }

        public BaseViewModel UpdatePortfolio(User user, UpdatePortfolioCommand cmd)
        {
            var vm = cPortHandler.UpdatePortfolio(cmd);
            return vm;
        }

        public BaseViewModel DeletePortfolio(User user, int portId)
        {
            var vm = cPortHandler.DeletePortfolio(portId);
            return vm;
        }
    }
}
