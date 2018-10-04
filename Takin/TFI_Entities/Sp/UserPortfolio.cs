using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.Entities.Sp
{
    public class UserPortfolio
    {
        public virtual int idPortfolio { get; set; }
        public virtual string strName { get; set; }
        public virtual double dEquity { get; set; }
        public virtual double currentEquity { get; set; }
        public virtual double dInitRisk { get; set; }
        public virtual double dCurrRisk { get; set; }
        //public virtual int iMaxSecs { get; set; }
        public virtual int iCalcPreference { get; set; }
        public virtual double PriceYesterday { get; set; }  // TODO: incorrect field, not used, DELETE later (SP:dataGetUserPortfolio)
        public virtual DateTime dtCreated { get; set; }
        public virtual DateTime dtLastOptimization { get; set; }
        public virtual string CalcCurrency { get; set; }
        public virtual double dLastProfit { get; set; }
        public virtual int iSecsNum { get; set; }
        public virtual long userId { get; set; }
        public virtual DateTime dtStartDate { get; set; }
        public virtual DateTime dtEndDate { get; set; }
    }
}
