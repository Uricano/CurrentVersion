using System;
using System.Collections.Generic;
using System.Text;
using FluentNHibernate.Mapping;
using TFI.Entities.Lookup;
using TFI.Entities.dbo;

namespace TFI.Entities.Mapping {
    
    
    public class UserlicensesMap : ClassMap<Userlicenses> {
        
        public UserlicensesMap()
        {
			Table("tblLic_UserLicenses");
			LazyLoad();
			Id(x => x.LicenseID).GeneratedBy.Identity().Column("LicenseID");
            References(x => x.tb_LicServices).Column("idLicService");
            References(x => x.User).Column("UserID");
            References(x => x.Licensetypes).Column("idLicenseType"); 
            References(x => x.Transaction).Column("ReceiptID"); 
            //Map(x => x.UserID).Column("UserID");
			//Map(x => x.ReceiptID).Column("ReceiptID");
			Map(x => x.dtExpirationDate).Column("dtExpirationDate");
			Map(x => x.dtActivationDate).Column("dtActivationDate");
			Map(x => x.dtPurchaseDate).Column("dtPurchaseDate");
			HasMany(x => x.Licenseexchanges).KeyColumn("LicenseID").Cascade.All(); 
        }
    }
}
