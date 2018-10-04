using System;
using System.Text;
using System.Collections.Generic;
using TFI.Entities.Lookup;

namespace TFI.Entities.dbo {
    
    public class UsersLogin
    {
        public UsersLogin() { }
        public virtual long ID { get; }
        public virtual long UserID { get; set; }
        public virtual DateTime LoginDT { get; set; }
        public virtual DateTime? LogoutDT { get; set; }
    }
}
