using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.Entities.dbo
{
    public class BacktestingPortfolioSecurities : EntityBase
    {
        public virtual Guid idPortSec { get; set; }
        public virtual BacktestingPortfolio Portfolios { get; set; }
        public virtual Security Securities { get; set; }
        public virtual bool? isActiveSecurity { get; set; }
        public virtual DateTime? dtStartDate { get; set; }
        public virtual DateTime? dtEndDate { get; set; }
        public virtual double? flWeight { get; set; }
        public virtual double? flRisk { get; set; }
        public virtual double? flRate { get; set; }
        public virtual double? flQuantity { get; set; }
        public virtual double? flLastPrice { get; set; }
        public virtual bool? isBenchmark { get; set; }
    }
}
