using System;
using System.Text;
using System.Collections.Generic;
using TFI.Entities.Lookup;

namespace TFI.Entities.dbo {
    
    public class Users
    {
        public Users()
        {
            Userlicenses = new List<Userlicenses>();
        }
        public virtual long UserID { get; set; }
        public virtual string Username { get; set; }
        public virtual string Password { get; set; }
        public virtual string Salt { get; set; }
        public virtual string Name { get; set; }
        //public virtual string LastName { get; set; }
        public virtual string Email { get; set; }
        public virtual bool? isTemporary { get; set; }
        public virtual bool IsLoggedin { get; set; }
        public virtual string LoggedinIP { get; set; }
        public virtual SelCurrency Currency { get; set; }
        public virtual string CellPhone { get; set; }
        public virtual IList<Userlicenses> Userlicenses { get; set; }
    }
}
