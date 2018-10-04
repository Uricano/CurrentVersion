using System;
using System.Text;
using System.Collections.Generic;


namespace TFI.Entities.dbo {
    
    public class TblBacktestingportfoliosecurities : EntityBase
    {
        public virtual System.Guid idPortSec { get; set; }
        public virtual bool? isActiveSecurity { get; set; }
        public virtual int? idPortfolio { get; set; }
        public virtual DateTime? dtStartDate { get; set; }
        public virtual DateTime? dtEndDate { get; set; }
        public virtual float? flWeight { get; set; }
        public virtual float? flRisk { get; set; }
        public virtual float? flRate { get; set; }
        public virtual string idSecurity { get; set; }
        public virtual float? flQuantity { get; set; }
        public virtual float? flLastPrice { get; set; }
        public virtual bool? isBenchmark { get; set; }
    }
}
