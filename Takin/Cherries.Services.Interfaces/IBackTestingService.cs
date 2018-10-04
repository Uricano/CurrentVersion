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
    public interface IBackTestingService : IServiceBase
    {
        BackTestingViewModel GetPortfolioBackTesting(User user, BackTestingQuery query);
        BackTestingViewModel calculateBacktesting(User user, Models.Command.CreatePortfolioCommand cmd, DateTime dtStartDate, DateTime dtEndDate, List<string> becnhMarkIDs);
        BackTestingViewModel GetBacktestingPortfolio(User user, int id);
        BaseViewModel IsPortfolioExists(string name, long userId);
    }
}
