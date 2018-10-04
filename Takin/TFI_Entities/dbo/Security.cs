using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFI.Entities.Lookup;

namespace TFI.Entities.dbo
{
    public class Security : EntityBase
    {
        public virtual string idSecurity { get; set; }
        public virtual string strSymbol { get; set; }
        public virtual string strName { get; set; }
        public virtual int? idMarket { get; set; }
        public virtual int? idSector { get; set; }
        public virtual DateTime? dtPriceEnd { get; set; }
        public virtual int? idSecurityType { get; set; }
        public virtual string idCurrency { get; set; }
        //public virtual string strISIN { get; set; }   //LR: there is no field in tbl_Securities
        //public virtual float? FAC { get; set; }
        public virtual float? dValue { get; set; }
        public virtual float? AvgYield { get; set; }
        public virtual float? StdYield { get; set; }
        public virtual string strHebName { get; set; }
        public virtual float? StdYieldNIS { get; set; }
        public virtual float? AvgYieldNIS { get; set; }
        public virtual float? MonetaryAvg { get; set; }
        public virtual float? MonetaryAvgNIS { get; set; }
        public virtual float? WeightUSA { get; set; }
        public virtual float? WeightNIS { get; set; }
        public virtual int? idSecurityRank { get; set; }
        public virtual IList<Price> Prices { get; set; }
        public virtual Sector Sectors { get; set; }
        public Security()
        {
            Prices = new List<Price>();
        }
    }
}
