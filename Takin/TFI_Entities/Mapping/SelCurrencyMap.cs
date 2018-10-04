using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.Entities.Lookup;

namespace TFI.Entities.Mapping
{
    public class SelCurrencyMap : ClassMap<SelCurrency>
    {
        public SelCurrencyMap()
        {
            Table("tblSel_Currencies");
            Id(x => x.CurrencyID).Column("idCurrency");
            Map(x => x.Name).Column("strName");
            Map(x => x.Sign).Column("strSign");
            Map(x => x.Symbol).Column("strSymbol");
            Map(x => x.IsActive).Column("isActive");
            Map(x => x.rank).Column("rank");
        }
    }
}
