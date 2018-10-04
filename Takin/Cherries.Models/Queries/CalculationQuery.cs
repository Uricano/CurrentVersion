using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.Queries
{
    public class CalculationQuery
    {
        public double[,] MatCovariance { get; set; }
        public double[] ArrReturns { get; set; }
    }
}
