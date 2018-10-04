using System;
using System.Collections.Generic;
using System.Text;
using FluentNHibernate.Mapping;
using TFI.Entities.dbo;

namespace TFI.Entities.Mapping {
    
    
    public class UserLoginMap : ClassMap<UsersLogin> {
        
        public UserLoginMap() {
			Table("tblLic_Logins");
			LazyLoad();
            Id(x => x.ID).GeneratedBy.Identity().Column("ID");
            Map(x => x.UserID).Column("UserID");
			Map(x => x.LoginDT).Column("LoginDT");
            Map(x => x.LogoutDT).Column("LogoutDT");
        }
    }
}
