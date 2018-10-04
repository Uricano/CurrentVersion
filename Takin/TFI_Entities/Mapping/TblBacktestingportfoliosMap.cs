using System; 
using System.Collections.Generic; 
using System.Text; 
using FluentNHibernate.Mapping;
using TFI.Entities.dbo; 

namespace TFI.Entities.Mapping {
    
    
    public class TblBacktestingportfoliosMap : ClassMap<TblBacktestingportfolios>
    {
         public TblBacktestingportfoliosMap()
        {
			Table("tbl_BacktestingPortfolios");
			LazyLoad();
			Id(x => x.idPortfolio).GeneratedBy.Identity().Column("idPortfolio");
			Map(x => x.strCode).Column("strCode");
			Map(x => x.strName).Column("strName");
			Map(x => x.iSecsNum).Column("iSecsNum");
			Map(x => x.dtLastOpened).Column("dtLastOpened");
			Map(x => x.dtCreated).Column("dtCreated");
			Map(x => x.dtStartDate).Column("dtStartDate");
			Map(x => x.dtEndDate).Column("dtEndDate");
			Map(x => x.dEquity).Column("dEquity");
			Map(x => x.dInitRisk).Column("dInitRisk");
			Map(x => x.idCurrency).Column("idCurrency");
			Map(x => x.iCalcPreference).Column("iCalcPreference");
			Map(x => x.isManual).Column("isManual");
			Map(x => x.dCurrRisk).Column("dCurrRisk");
			Map(x => x.dtLastOptimization).Column("dtLastOptimization");
			Map(x => x.dCurrEquity).Column("dCurrEquity");
			Map(x => x.userId).Column("userId");
        }
    }
}
