using Cherries.Models.App;
using Cherries.Models.Command;
using Cherries.Models.dbo;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace TFI.BusinessLogic.Interfaces
{
    public interface IManageBacktestingPortfolios : IBaseBL
    {
        List<PortfolioDetails> GetPortfolioDetailsList(long userId);
        TopPortfoliosViewModel GetPortfolioDetailsList(GetPortfolioQuery query);
        BacktestingPortfolioViewModel GetFullPortfolio(int id, string currency, List<int> exchangesPackagees);
        List<String> getPortfolioBenchmarkNames(long portID);
        void openSelectedPortfolio(int iPortID, bool isBacktPort);
        int CreateDefaultPortfolio(User user, DateTime dtStartDate, DateTime dtEndDate, CreatePortfolioCommand cmd);
        int GetPortfolioCount(long userID);
        bool IsPortfolioExists(string name, long userId);
        BaseViewModel UpdatePortfolio(UpdatePortfolioCommand cmd, BackTestingViewModel vmBack);
        BaseViewModel DeletePortfolio(int PortID);
        IPortfolioBL SelectedPortfolio { get; }
    }
}
