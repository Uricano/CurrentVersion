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
    public interface IPortfolioService : IServiceBase
    {
        PortfolioViewModel GetPortolios(long userId);
        TopPortfoliosViewModel GetPortolios(GetPortfolioQuery query);
        PortfolioViewModel GetFullPortfolio(User user, int id);
        OptimalPortoliosViewModel CreatePortfolio(User user, CreatePortfolioCommand cmd);
        BaseViewModel UpdatePortfolio(User user, UpdatePortfolioCommand cmd);
        BaseViewModel DeletePortfolio(User user, int portId);
        BaseViewModel IsMaxPortfolioExceeded(User user);
        BaseViewModel IsPortfolioExists(string name, long userId);
        
    }
}
