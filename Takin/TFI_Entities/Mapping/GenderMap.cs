using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.Entities.Lookup;

namespace TFI.Entities.Mapping
{
    public class GenderMap : ClassMap<Gender>
    {
        public GenderMap()
        {
            Table("tblSel_Gender");
            LazyLoad();
            Id(x => x.Id).Column("Id");
            Map(x => x.Description).Column("Description");
        }
    }
}
