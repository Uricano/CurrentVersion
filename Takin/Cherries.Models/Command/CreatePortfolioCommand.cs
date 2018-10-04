using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.BusinessLogic.Enums;

namespace Cherries.Models.Command
{
    public class CreatePortfolioCommand : BasePortfolioCommand
    {
        public string Name { get; set; }
        public List<string> Securities { get; set; }
        public List<int> Exchanges { get; set; }
    }
}
