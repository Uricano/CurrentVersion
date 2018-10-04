using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.Lookup
{
    public class StockMarket
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string HebName { get; set; }
        public string Currency { get; set; }
        public string CurrencyRank { get; set; }
    }
}
