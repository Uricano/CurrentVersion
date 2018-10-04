using Cherries.Models.ViewModel;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.Optimization;
using Cherries.TFI.BusinessLogic.Securities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace TFI.BusinessLogic.Interfaces
{
    public interface IOptimizationResults : IBaseBL
    {
        //OptimalPortoliosViewModel calculateEfficientFrontier(ISecurities cSecsCol, ISecurities cBenchmarksCol, cDateRange drCalcRange, Boolean isWithConsts, Boolean isClearDisabled);
        OptimalPortoliosViewModel calculateNewEfficientFrontier(ref ISecurities cSecsCol, ISecurities cBenchmarksCol, cDateRange drCalcRange, Boolean isClearDisabled, Boolean isBacktest);
        //int getClosestRiskValIndex(double riskVal);
        void setCollectionsDataForRiskLevel(int iRiskInd);
        void setSecuritiesWeightsCollection(ISecurities cSecsCol, int iRiskInd);
        //ISecurities SecuritiesCol { get; set; }
        Boolean isSuccessful { get; set; }
        //PointF RiskRange { get; }
        List<cOptimalPort> Portfolios { get; }
        int PortNumA { get; set; }
        int OrigPortPos
        {
            get;
            set;
        }//OrigPortPos

        int TangencyPos
        { get; }

        double TangencyPortRisk
        { get; }//TangencyPortRisk
        
    }
}
