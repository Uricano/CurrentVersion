using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.Queries
{
    public class LogoffCommand
    {
        public List<long> Users { get; set; }
        public string IP { get; set; }
    }
}
