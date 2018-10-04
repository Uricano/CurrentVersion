using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.BusinessLogic.Enums;

namespace Cherries.Models.Queries
{
    public class OptimazationQuery
    {
        public int PortID { get; set; }
        public enumEfCalculationType CalcType { get; set; }
        public List<string> Securities { get; set; }
        public List<int> Exchanges { get; set; }
        public double Risk { get; set; }
        //public string curreny { get; set; }
        //public int numSecs { get; set; }
        //public List<int> ExchangesPackagees { get; set; }
    }
}
