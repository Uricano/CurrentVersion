using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.Entities.dbo
{
    public class PortfolioBase : EntityBase
    {
        public virtual int idPortfolio { get; set; }
        public virtual string strCode { get; set; }
        public virtual string strName { get; set; }
        public virtual int? iSecsNum { get; set; }
        public virtual DateTime? dtLastOpened { get; set; }
        public virtual DateTime? dtCreated { get; set; }
        public virtual DateTime? dtStartDate { get; set; }
        public virtual DateTime? dtEndDate { get; set; }
        public virtual double? dEquity { get; set; }
        public virtual double? dInitRisk { get; set; }
        //public virtual int? iMaxSecs { get; set; }
        public virtual string CalcCurrency { get; set; }
        public virtual int? iCalcPreference { get; set; }
        public virtual bool? isManual { get; set; }
        public virtual double? dCurrRisk { get; set; }
        public virtual DateTime? dtLastOptimization { get; set; }
        public virtual double? dCurrEquity { get; set; }
        public virtual long userId { get; set; }
    }
}
