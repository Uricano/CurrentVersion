using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.Entities.Lookup;

namespace TFI.Entities.Mapping
{
    public class StockMarketViewMap : ClassMap<LookupBase>
    {
        public StockMarketViewMap()
        {
            Table("v_tbSel_StockMarkets");
            LazyLoad();
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.strName).Column("strName");
            Map(x => x.strHebName).Column("strHebName");
            Map(x => x.IsActive).Column("isActive");
        }
    }
}
