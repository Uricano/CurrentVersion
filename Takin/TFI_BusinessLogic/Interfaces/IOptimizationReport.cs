using Cherries.Models.App;
using Cherries.Models.dbo;
using Cherries.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFI.BusinessLogic.Interfaces
{
    public interface IOptimizationReport : IBaseBL
    {
        FileDataViewModel Report(int portId, User user, OptimalPortfolio opPort);
    }
}
