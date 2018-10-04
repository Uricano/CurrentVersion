using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.dbo
{
    public class Security
    {
        public string idSecurity { get; set; }
        public string strSymbol { get; set; }
        public string strName { get; set; }
        public int idMarket { get; set; }
        public int idSector { get; set; }
        public string marketName { get; set; }
        public string sectorName { get; set; }
        public string securityTypeName { get; set; }
        public DateTime dtPriceStart { get; set; }
        public DateTime dtPriceEnd { get; set; }
        public int idSecurityType { get; set; }
        public string idCurrency { get; set; }
        //public string strISIN { get; set; }   //LR: no field
        //public double FAC { get; set; }       //LR: no field
        public double AvgYield { get; set; }
        public double StdYield { get; set; }
        public string strHebName { get; set; }
        public double StdYieldNIS { get; set; }
        public double AvgYieldNIS { get; set; }
        public double dValueUSA { get; set; }
        public double dValueNIS { get; set; }
        public double WeightUSA { get; set; }
        public double WeightNIS { get; set; }
        public bool? isActiveSecurity { get; set; }
        public int idSecurityRank { get; set; }
        public Int64 rank { get; set; }
        public DateTime? dtStartDate { get; set; }
        public DateTime? dtEndDate { get; set; }
        public IList<Price> Prices { get; set; }
    }
}
