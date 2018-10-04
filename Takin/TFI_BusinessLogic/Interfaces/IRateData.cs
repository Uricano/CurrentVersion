using Cherries.Models.App;
using Cherries.Models.dbo;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TFI.BusinessLogic.Interfaces
{
    public interface IRateData : IBaseBL
    {
        void setUpdatedRateData(cDateRange drPeriod);
        void setFinalReturn(double dFinalReturn);
        void setFinalReturnFromServer(cDateRange drPeriod);
        List<Rate> RatesData { get; }
        double FinalRate { get; }
        double ExpectedReturnAvg { get; }
        List<PriceReturn> PriceReturns { get; set; }
        List<PriceReturn> BacktestingReturns { get; set; }
        Boolean isMinimized { get; set; }
        double RateAvgOneYear { get; }
        double WeeklyReturn { get; }
    }
}
