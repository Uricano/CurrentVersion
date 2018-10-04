using Cherries.Models.dbo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.Command
{
    public class SaveUserCommand
    {
        public User User;
        public string Password { get; set; }
        public double SumInServer { get; set; }
        public string Cupon { get; set; }
    }
}
