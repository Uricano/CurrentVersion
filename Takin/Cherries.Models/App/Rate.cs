using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.App
{
    public class Rate
    {
        public DateTime Date { get; set; }
        public double? RateVal { get; set; }
        public double? MiniRate { get; set; }
        public string Label { get; set; }
        public string Tooltip { get; set; }
    }
}
