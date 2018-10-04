using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.dbo
{
    public class PortfolioSecurities
    {
        public int idPortSec { get; set; }
        public Security Securities { get; set; }
        public bool? isActiveSecurity { get; set; }
        public DateTime? dtStartDate { get; set; }
        public DateTime? dtEndDate { get; set; }
    }
}
