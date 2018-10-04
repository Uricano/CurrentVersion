using Cherries.Models.App;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Cherries.Models.ViewModel
{
    public class BackTestingViewModel : BaseViewModel
    {
        public DataTable SecuritiesTable { get; set; }
        public double Equity { get; set; }
        public double Profit { get; set; }
        public String CurrencySign { get; set; }
        public double AdjCoeff { get; set; }
        public double BenchmarkRisk { get;
            set; }
        public double CurrPortRiskVal { get; set; }
        public double CurrPortReturnValue { get; set; }
        public double CurrPortRateToRisk { get; set; }
        public double CurrPortDiversification { get; set; }
        public String BenchmarkID { get; set; }
        public List<Tuple<GeneralItem, List<BenchMarkResult>>> benchMarkResult { get; set; }

        public List<OptimalPortfolio> Portfolios { get; set; }
        public int PortNumA { get;  set; }
    }
}
