using System; 
using System.Collections.Generic; 
using System.Text; 
using FluentNHibernate.Mapping;
using TFI.Entities.dbo; 

namespace TFI.Entities.Mapping {
    
    
    public class TblBacktestingportfoliosecuritiesMap : ClassMap<TblBacktestingportfoliosecurities>
    {
        
        public TblBacktestingportfoliosecuritiesMap()
        {
			Table("tbl_BacktestingPortfolioSecurities");
			LazyLoad();
			Id(x => x.idPortSec).GeneratedBy.Assigned().Column("idPortSec");
			Map(x => x.isActiveSecurity).Column("isActiveSecurity");
			Map(x => x.idPortfolio).Column("idPortfolio");
			Map(x => x.dtStartDate).Column("dtStartDate");
			Map(x => x.dtEndDate).Column("dtEndDate");
			Map(x => x.flWeight).Column("flWeight");
			Map(x => x.flRisk).Column("flRisk");
			Map(x => x.flRate).Column("flRate");
			Map(x => x.idSecurity).Column("idSecurity");
			Map(x => x.flQuantity).Column("flQuantity");
			Map(x => x.flLastPrice).Column("flLastPrice");
			Map(x => x.isBenchmark).Column("isBenchmark");
        }
    }
}
