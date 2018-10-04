using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.App
{
    public class PriceReturn
    {

        public int idPriceReturn { get; set; }
        public string idSecurity { get; set; }
        public int idCurrency { get; set; }
        public DateTime dtDate { get; set; }
        public double? dReturn { get; set; }
        public double? dMiniReturn { get; set; }
        public double? fAdjClose { get; set; }
        public double? fAdjClosePrevWeek { get; set; }

    }//Of class
}
