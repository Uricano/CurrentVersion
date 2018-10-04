using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.App
{
    public class OptimalPortfolioSecurity
    {
        public double? Weight { get; set; }
        public double? Quantity { get; set; }
        public double Value { get; set; }
        public double DisplayValue { get; set; }
        public string idSecurity { get; set; }
        public int idMarket { get; set; }
        public int idSector { get; set; }
        public string marketName { get; set; }
        public string sectorName { get; set; }
        public string securityTypeName { get; set; }
        public DateTime? dtPriceStart { get; set; }
        public DateTime? dtPriceEnd { get; set; }
        public int idSecurityType { get; set; }
        public string Name { get; set; }
        public string HebName { get; set; }
        public string Symbol { get; set; }
        public double? StandardDeviation { get; set; }
        public double? StdYield { get; set; }
        public double? FinalRate { get; set; }
        public double? LastPrice { get; set; }
        public string IdCurrency { get; set; }
    }
}
