using Cherries.Models.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.Queries
{
    public class OptimazationExportQuery
    {
        public int PortID { get; set; }
        public OptimalPortfolio OptimalPort { get; set; }
    }
}
