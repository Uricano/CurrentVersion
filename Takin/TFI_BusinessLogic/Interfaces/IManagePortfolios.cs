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
    public interface IManagePortfolios : IBaseBL
    {
        List<PortfolioDetails> GetPortfolioDetailsList(long userId);
        TopPortfoliosViewModel GetPortfolioDetailsList(GetPortfolioQuery query);
        PortfolioViewModel GetFullPortfolio(int id, string currency, List<int> exchangesPackagees);
        PortfolioViewModel openPortfolio(int id, string currency, List<int> exchangesPackagees);
        int CreateDefaultPortfolio(User user, CreatePortfolioCommand cmd);
        int GetPortfolioCount(long userID);
        bool IsPortfolioExists(string name, long userId);
        BaseViewModel UpdatePortfolio(UpdatePortfolioCommand cmd);
        BaseViewModel DeletePortfolio(int PortID);
        IPortfolioBL SelectedPortfolio { get; }       
    }
}
