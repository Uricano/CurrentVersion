using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Cherries.Models.App
{
    public class OptimalPortfolio
    {
        public double Risk { get; set; }
        public double Return { get; set; }
        public double RateToRisk { get; set; }
        public double Diversification { get; set; }
        public double Sharpe { get; set; }
        public List<OptimalPortfolioSecurity> Securities { get; set; }
        public double Cash { get; set; }
    }
}
