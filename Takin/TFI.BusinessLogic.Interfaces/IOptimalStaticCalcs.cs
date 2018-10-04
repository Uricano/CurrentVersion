using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.BusinessLogic.Interfaces
{
    public interface IOptimalStaticCalcs : IBaseBL
    {
        String runOptimizationProcess(out List<double> colRisks, out List<double> colReturns, out double[,] matWeights, double[,] dCovarMatrix, double[] dRatesVector,
            double[] arrUpperBound, double[] arrLowerBound, List<List<double>> colEqualitySecs, double[] arrEqualityVals, int iEqualityNum, List<List<double>> colInequalitySecs,
            double[] arrInequalityVals, int iEps, int iLoopsMax);
    }
}
