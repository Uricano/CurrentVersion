using Cherries.Models.App;
using Cherries.Models.dbo;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Cherries.Services.Interfaces
{
    public interface IOptimizationService : IServiceBase
    {
        OptimalPortoliosViewModel GetPortfolioOptimazation(User user, OptimazationQuery query);
        FileDataViewModel Export(int portId, User user, OptimalPortfolio opPort);
        void UpdateSecurities(HttpContext current);
    }
}
