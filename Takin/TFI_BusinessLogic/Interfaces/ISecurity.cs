using Cherries.Models.App;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.Securities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TFI.BusinessLogic.Interfaces
{
    public interface ISecurity : IBaseBL
    {
        void disableCurrentSecurity();
        void enableCurrentSecurity();
        void setSecurityActivity(Boolean isActive);
        void Init(IPortfolioBL cPort);
        void clearOptimizationData();
        cSecProperties Properties { get; set; }
        cSecAnalytics Analytics { get; set; }
        cDateRange DateRange { get; set; }
        List<Entities.dbo.Price> PriceTable { get; set; }
        IRateData RatesClass { get; }
        List<Rate> RatesTable { get; }
        ICovarCorrelData CovarClass { get; }
        double Weight { get; set; }
        double Quantity { get; set; }
        double LastPrice { get; set; }
        double FAC { get; set; }
        string IdCurrency { get; set; }
        double AvgYield { get; set; }
        double StdYield { get; set; }
        double AvgYieldNIS { get; set; }
        double StdYieldNIS { get; set; }
        double ValueUSA { get; set; }
        double ValueNIS { get; set; }
        double WeightUSA { get; set; }
        double WeightNIS { get; set; }
        Boolean isActive { get; }
        IPortfolioBL Portfolio { get; set; }

    }
}
