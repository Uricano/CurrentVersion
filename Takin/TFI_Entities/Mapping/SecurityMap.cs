using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFI.Entities.dbo;

namespace TFI.Entities.Mapping
{
    public class SecurityMap : ClassMap<Security>
    {

        public SecurityMap()
        {
            Table("v_Securities");
            ReadOnly();
            Id(x => x.idSecurity).Column("IdSecurity");
            //Map(x => x.idSecurity).Column("idSecurity");
            Map(x => x.strSymbol).Column("strSymbol");
            Map(x => x.strName).Column("strName");
            Map(x => x.idMarket).Column("idMarket");
            Map(x => x.idSector).Column("idSector");
            Map(x => x.dtPriceEnd).Column("dtPriceEnd");
            Map(x => x.idSecurityType).Column("idSecurityType");
            Map(x => x.idCurrency).Column("idCurrency");
            //Map(x => x.strISIN).Column("strISIN");
            //Map(x => x.FAC).Column("FAC");
            Map(x => x.dValue).Column("dValue");
            Map(x => x.AvgYield).Column("avgYield");
            Map(x => x.StdYield).Column("stdYield");
            Map(x => x.strHebName).Column("strHebName");
            Map(x => x.StdYieldNIS).Column("stdYieldNIS");
            Map(x => x.AvgYieldNIS).Column("avgYieldNIS");
            Map(x => x.MonetaryAvg).Column("MonetaryAvg");
            Map(x => x.MonetaryAvgNIS).Column("MonetaryAvgNIS");
            Map(x => x.WeightUSA).Column("WeightUSA");
            Map(x => x.WeightNIS).Column("WeightNIS");
            Map(x => x.idSecurityRank).Column("idSecurityRank");
            References(x => x.Sectors).Column("idSector");
            HasMany(x=> x.Prices).Table("tbl_Prices").KeyColumn("idSecurity").Cascade.All();
        }
    }
}
