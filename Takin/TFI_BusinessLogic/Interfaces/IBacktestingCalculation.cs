using Cherries.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.BusinessLogic.Interfaces
{
    public interface IBacktestingCalculation : IBaseBL
    {
        BackTestingViewModel calculateBacktesting(IPortfolioBL cPort, DateTime dtStartDate, DateTime dtEndDate, double dEquity, String idBenchmark);
    }
}
