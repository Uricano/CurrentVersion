using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.ViewModel
{
    public class CalculationResponse
    {
        public List<double> colRisks { get; set; }
        public List<double> colReturns { get; set; }
        public double[,] matWeights { get; set; }
        public string errorString { get; set; }
    }
}
