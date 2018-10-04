using Cherries.Models.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.dbo
{
    public class User
    {
        public long UserID { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public Currency Currency { get; set; }
        public bool isTemporary { get; set; }
        public bool IsLoggedin { get; set; }
        public virtual string CellPhone { get; set; }
        public UserLicence Licence { get; set; }
        public User()
        {
            Licence = new dbo.UserLicence();
        }
    }
}
