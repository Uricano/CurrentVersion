using Cherries.Models.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.Queries
{
    public class BackTestingQuery
    {
        public int PortID { get; set; }
        //public string curreny { get; set; }
        //public int numSecs { get; set; }
        //public List<int> ExchangesPackagees { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<String> BenchMarkID { get; set; }
        public CreatePortfolioCommand PortfolioCommand { get; set; }
    }
}
