using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.App
{
    public class BenchMarkResult
    {
        public DateTime StartDate { get; set; }
        public double PortAmount { get; set; }
        public double PortReturn { get; set; }
        public double IndexAmount { get; set; }
        public double IndexReturn { get; set; }

        public BenchMarkResult() { }
        public BenchMarkResult(DateTime startDate, double portAmount, double portReturn, double indexAmount, double indexReturn)
        {
            StartDate = startDate;
            PortAmount = portAmount;
            PortReturn = portReturn;
            IndexAmount = indexAmount;
            IndexReturn = indexReturn;
        }
    }
}
