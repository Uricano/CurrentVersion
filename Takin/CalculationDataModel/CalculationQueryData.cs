using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalculationDataModel
{
    public class CalculationQueryData
    {
        public int VarSize { get; set; }
        public double[,] MatCovariance { get; set; }
        public double[] ArrReturns { get; set; }
    }

    
}
