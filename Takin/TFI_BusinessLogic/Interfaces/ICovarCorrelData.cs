using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.BusinessLogic.Interfaces
{
    public interface ICovarCorrelData : IBaseBL
    {
        void setCalculationStatistics(Boolean isBacktest);
        //void calcRiskLevels();
        double SumRates { get; set; }
        double Variance { get; set; }
        double StandardDeviation { get; set; }
        double StDevWeekly { get; set; }
        double Average { get; }
    }
}
