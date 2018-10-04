using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.Entities.dbo;

namespace TFI.Entities.Mapping
{
    public class TopSecuritiesMap : ClassMap<TopSecurities>
    {
        public TopSecuritiesMap()
        {
            Table("v_SecuritiesWithRank");
            LazyLoad();
            Id(x => x.idSecurity).Column("idSecurity");
            Map(x => x.strSymbol).Column("strSymbol");
            Map(x => x.strName).Column("strName");
            Map(x => x.strHebName).Column("strHebName");
            Map(x => x.idMarket).Column("idMarket");
            Map(x => x.idSector).Column("idSector");
            Map(x => x.dtPriceStart).Column("dtPriceStart");
            Map(x => x.dtPriceEnd).Column("dtPriceEnd");
            Map(x => x.idSecurityType).Column("idSecurityType");
            Map(x => x.idCurrency).Column("idCurrency");
            //Map(x => x.strISIN).Column("strISIN");    //LR:
            Map(x => x.dValue).Column("dValue");
            //Map(x => x.FAC).Column("FAC");
            Map(x => x.StdYield).Column("StdYield");
            Map(x => x.AvgYield).Column("AvgYield");
            Map(x => x.StdYieldNIS).Column("StdYieldNIS");
            Map(x => x.AvgYieldNIS).Column("AvgYieldNIS");
            Map(x => x.MonetaryAvgNIS).Column("MonetaryAvgNIS");
            Map(x => x.MonetaryAvg).Column("MonetaryAvg");
            Map(x => x.idSecurityRank).Column("idSecurityRank");
            Map(x => x.marketName).Column("marketName");
            Map(x => x.sectorName).Column("sectorName");
            Map(x => x.securityTypeName).Column("securityTypeName");
            Map(x => x.rank).Column("rank");
        }
    }
}
