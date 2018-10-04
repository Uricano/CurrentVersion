using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.Entities.Lookup;

namespace TFI.Entities.Mapping
{
    public class CountryMap : ClassMap<Country>
    {
        public CountryMap()
        {
            Table("tbSel_Countries");
            LazyLoad();
            Id(x => x.CountryID).Column("CountryID");
            Map(x => x.CountryName).Column("CountryName");
        }
    }
}
