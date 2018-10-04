using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.BusinessLogic.Enums;

namespace Cherries.Models.Command
{
    public class BasePortfolioCommand
    {
        public double Equity { get; set; }
        public double Risk { get; set; }
        public enumEfCalculationType CalcType { get; set; }
    }
}
