using System;
using System.Text;
using System.Collections.Generic;


namespace TFI.Entities.dbo {
    
    public class TblBacktestingportfolios : EntityBase
    {
        public virtual int idPortfolio { get; set; }
        public virtual string strCode { get; set; }
        public virtual string strName { get; set; }
        public virtual int? iSecsNum { get; set; }
        public virtual DateTime? dtLastOpened { get; set; }
        public virtual DateTime? dtCreated { get; set; }
        public virtual DateTime? dtStartDate { get; set; }
        public virtual DateTime? dtEndDate { get; set; }
        public virtual float? dEquity { get; set; }
        public virtual float? dInitRisk { get; set; }
        public virtual string idCurrency { get; set; }
        public virtual int? iCalcPreference { get; set; }
        public virtual bool? isManual { get; set; }
        public virtual float? dCurrRisk { get; set; }
        public virtual DateTime? dtLastOptimization { get; set; }
        public virtual float? dCurrEquity { get; set; }
        public virtual long? userId { get; set; }
    }
}
