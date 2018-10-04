using System;
using System.Collections.Generic;
using System.Text;
using FluentNHibernate.Mapping;
using TFI.Entities.dbo;

namespace TFI.Entities.Mapping {
    
    
    public class UsersMap : ClassMap<Users> {
        
        public UsersMap() {
			Table("tblLic_Users");
			LazyLoad();
			Id(x => x.UserID).GeneratedBy.Identity().Column("UserID");
			Map(x => x.Username).Column("strUsername");
			Map(x => x.Password).Column("strPassword");
            Map(x => x.Salt).Column("Salt");
            Map(x => x.Name).Column("strName");
			//Map(x => x.LastName).Column("LastName");
			Map(x => x.Email).Column("strEmail");
			Map(x => x.isTemporary).Column("isTemporary");
            Map(x => x.IsLoggedin).Column("isLoggedin");
            Map(x => x.LoggedinIP).Column("loggedinIP");
            Map(x => x.CellPhone).Column("cellPhone");
            References(x => x.Currency).Column("idCurrency");
            HasMany(x => x.Userlicenses).KeyColumn("UserID"); 
        }
    }
}
