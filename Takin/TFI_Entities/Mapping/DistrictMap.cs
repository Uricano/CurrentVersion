using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.Entities.Lookup;

namespace TFI.Entities.Mapping
{
    public class DistrictMap : ClassMap<District>
    {
        public DistrictMap()
        {
            Table("tblSel_Districts");
            LazyLoad();
            Id(x => x.idDistrict).GeneratedBy.Identity().Column("idDistrict");
            References(x => x.Country).Column("idCountry");
            Map(x => x.strDistrictName).Column("strDistrictName");
        }
    }
}
