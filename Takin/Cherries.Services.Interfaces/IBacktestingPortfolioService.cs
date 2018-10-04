using Cherries.Models.Command;
using Cherries.Models.dbo;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cherries.Services.Interfaces
{
    public interface IBacktestingPortfolioService : IServiceBase
    {

        //BacktestingPortfolioViewModel GetPortolios(long userId);
        BacktestingPortsViewModel GetPortolios(GetPortfolioQuery query);
        BacktestingPortfolioViewModel GetFullPortfolio(User user, int id);
       
        BaseViewModel IsMaxPortfolioExceeded(User user);
       
        BaseViewModel IsPortfolioExists(string name, long userId);

        BaseViewModel DeletePortfolio(User user, int portId);
    }
}
