using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.Entities.dbo
{
    public class TopSecurities
    {
        public virtual string idSecurity { get; set; }
        public virtual string strSymbol { get; set; }
        public virtual string strName { get; set; }
        public virtual string strHebName { get; set; }
        public virtual int idMarket { get; set; }
        public virtual int idSector { get; set; }
        public virtual string marketName { get; set; }
        public virtual string sectorName { get; set; }
        public virtual string securityTypeName { get; set; }
        public virtual DateTime dtPriceStart { get; set; }
        public virtual DateTime dtPriceEnd { get; set; }
        public virtual int idSecurityType { get; set; }
        public virtual string idCurrency { get; set; }
        //public virtual string strISIN { get; set; }   //LR: no field
        public virtual double dValue { get; set; }
        //public virtual double FAC { get; set; }       //LR: no field
        public virtual double StdYield { get; set; }
        public virtual double AvgYield { get; set; }
        public virtual double StdYieldNIS { get; set; }
        public virtual double AvgYieldNIS { get; set; }
        public virtual double MonetaryAvgNIS { get; set; }
        public virtual double MonetaryAvg { get; set; }
        public virtual double WeightNIS { get; set; }
        public virtual double WeightUSA { get; set; }
        public virtual int idSecurityRank { get; set; }
        public virtual Int64 rank { get; set; }
    }
}
