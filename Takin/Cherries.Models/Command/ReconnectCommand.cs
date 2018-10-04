using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.Command
{
    public class ReconnectCommand
    {
        public long UserID { get; set; }
        public string IP { get; set; }
    }
}
