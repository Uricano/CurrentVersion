using Cherries.Models.App;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Cherries.Models.ViewModel
{
    public class OptimalPortoliosViewModel : BaseViewModel
    {
        public DataTable SecuritiesTable { get; set; }
        public double Cash { get; set; }
        public Boolean isSuccessful { get; set; }
        public int PortNumA { get; set; }
        public int OrigPortPos { get; set; }
        public int TangencyPos { get; set; }
        public double TangencyPortRisk { get; set; }
        public List<OptimalPortfolio> Portfolios { get; set; }
        public String EngineError { get; set; }
        public PointF RiskRange { get; set; }
        public int PortID { get; set; }
        public PortfolioDetails PortDetails { get; set; }
    }
}
