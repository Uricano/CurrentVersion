using Cherries.Models.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.Command
{
    public class UpdatePortfolioCommand : BasePortfolioCommand
    {
        public int PortID { get; set; }
        public List<OptimalPortfolioSecurity> Securities { get; set; }
    }
}
