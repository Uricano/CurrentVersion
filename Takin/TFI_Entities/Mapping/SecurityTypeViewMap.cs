using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.Entities.Lookup;

namespace TFI.Entities.Mapping
{
    public class SecurityTypeViewMap : ClassMap<SelSecurityType>
    {
        public SecurityTypeViewMap()
        {
            Table("v_tbSel_SecurityTypes");
            LazyLoad();
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.strName).Column("strName");
            Map(x => x.strHebName).Column("strNameHeb");
            Map(x => x.IsActive).Column("isActive");
        }
    }
}
