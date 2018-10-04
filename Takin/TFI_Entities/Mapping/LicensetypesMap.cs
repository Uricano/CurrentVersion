using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.Entities.Lookup;

namespace TFI.Entities.Mapping
{
    public class LicensetypesMap : ClassMap<Licensetypes>
    {
        public LicensetypesMap()
        {
            Table("tblLic_LicenseTypes");
            LazyLoad();
            Id(x => x.Idlicensetype).GeneratedBy.Identity().Column("idLicenseType");
            Map(x => x.Strname).Column("strName");
        }
    }
}
