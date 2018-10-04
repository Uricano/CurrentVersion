using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.Entities.Lookup;

namespace TFI.Entities.Mapping
{
    public class LicenseexchangesMap : ClassMap<Licenseexchanges>
    {
        public LicenseexchangesMap()
        {
            Table("tblLic_LicenseExchanges");
            LazyLoad();
            Id(x => x.Idlicexchanges).GeneratedBy.Identity().Column("idLicExchanges");
            References(x => x.Userlicenses).Column("LicenseID");
            References(x => x.Stockexchanges).Column("idStockExchange");
        }
    }
}
