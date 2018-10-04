using System;
using System.Collections.Generic;
using System.Text;
using FluentNHibernate.Mapping;
using TFI.Entities.Lookup;
using TFI.Entities.dbo;

namespace TFI.Entities.Mapping {
    
    
    public class LicservicesMap : ClassMap<Licservices> {
        
        public LicservicesMap() {
			Table("tblLic_LicServices");
            //LazyLoad();
            Id(x => x.Idlicservice).GeneratedBy.Identity().Column("idLicService");
            Map(x => x.StrServiceType).Column("strServiceType");
            Map(x => x.Strservicename).Column("strServiceName");
            Map(x => x.Imonths).Column("iMonths");
            Map(x => x.Iportfolios).Column("iPortfolios");
            Map(x => x.Dstartprice).Column("dStartPrice");
            Map(x => x.Dnewexchangeprice).Column("dNewExchangePrice");
            Map(x => x.Ibaseexchanges).Column("iBaseExchanges");
            Map(x => x.IsTrial).Column("IsTrial");
        }
    }
}
