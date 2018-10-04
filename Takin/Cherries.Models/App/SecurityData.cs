using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.App
{
    public class SecurityData
    {
        public virtual string idSecurity { get; set; }
        public virtual string strSymbol { get; set; }
        public virtual string strName { get; set; }
        public virtual int idMarket { get; set; }
        public virtual int idSector { get; set; }
        public virtual int idSecurityType { get; set; }
        public virtual string idCurrency { get; set; }
        //public virtual float FAC { get; set; }        //LR: no field
        public virtual double avgYield { get; set; }
        public virtual double stdYield { get; set; }
        public virtual string strHebName { get; set; }
        public virtual double stdYieldNIS { get; set; }
        public virtual double avgYieldNIS { get; set; }
        public virtual double dValueUSA { get; set; }
        public virtual double dValueNIS { get; set; }
        public virtual double WeightUSA { get; set; }
        public virtual double WeightNIS { get; set; }
        public virtual double flQuantity { get; set; }
        public virtual double flLastPrice { get; set; }
        public virtual double flYesterdayPrice { get; set; }
        public virtual double flCreationPrice { get; set; }
        public virtual double portSecWeight { get; set; }
        public virtual string marketName { get; set; }
        public virtual string securityTypeName { get; set; }
        public virtual string sectorName { get; set; }
        public virtual bool isSelected { get; set; }
        public virtual double SecValue { get; set; }
        public virtual double Profit { get; set; }

    }
}
