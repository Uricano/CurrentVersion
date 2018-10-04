using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.Optimization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TFI.BusinessLogic.Interfaces
{
    public interface ICovarCorrelHandler : IBaseBL
    {
        //double[,] runMainCalculation(ISecurities cSecsCol, cDateRange drPeriod, Boolean isRemoveDisabled, Boolean isBench);
        double[,] calcRiskAndCovariance(cDateRange drPeriod, ref ISecurities cSecsCol, Boolean isMatrix, Boolean isBench);
        void reportCovarianceMatrix(ISecurities cSecsCol, DataTable dtResults, cOptimalPort cPort, double dEquity, Boolean isBacktesting);
    }
}
