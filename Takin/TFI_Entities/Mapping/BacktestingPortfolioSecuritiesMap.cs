using System;
using System.Collections.Generic;
using System.Text;
using FluentNHibernate.Mapping;
using TFI.Entities.dbo;


namespace TFI.Entities.Mapping
{
    public class BacktestingPortfolioSecuritiesMap : ClassMap<BacktestingPortfolioSecurities>
    {
        public BacktestingPortfolioSecuritiesMap()
        {
            Table("tbl_BacktestingPortfolioSecurities");

            Id(x => x.idPortSec).Column("idPortSec");
            References(x => x.Portfolios).Column("idPortfolio");
            References(x => x.Securities).Column("idSecurity");
            Map(x => x.isActiveSecurity).Column("isActiveSecurity");
            Map(x => x.dtStartDate).Column("dtStartDate");
            Map(x => x.dtEndDate).Column("dtEndDate");
            Map(x => x.flWeight).Column("flWeight");
            Map(x => x.flRisk).Column("flRisk");
            Map(x => x.flRate).Column("flRate");
            Map(x => x.flQuantity).Column("flQuantity");
            Map(x => x.flLastPrice).Column("flLastPrice");
            Map(x => x.isBenchmark).Column("isBenchmark");
        }
    }
}
