using Cherries.Models.App;
using Cherries.TFI.BusinessLogic.Collections;
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Portfolio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Cherries.Models.App.PortRiskItem;

namespace TFI.BusinessLogic.Interfaces
{
    public interface IPortfolioBL : IBaseBL
    {
        //bool instantiatePortfolioVariables(Boolean isFullProcess, string currency, List<int> exchangesPackagees, List<string> securities = null);
        bool instantiateVariablesForPortfolio(Boolean isCreateNew, string sCurrency, List<int> exchangesIds, List<string> securities = null);
        void openExistingPortfolio(int iPortID, bool isBacktPort);
        void InitCollectionObject(string currency, List<int> exchangesPackagees);
        cPortRiskItem GetRisk(double risk);
        IErrorHandler cErrorLog { get; }
        ICollectionsHandler ColHandler { get; set; }
        cPortfolioClasses Classes { get; set; }
        PortfolioDetails Details { get; set; }
        List<Entities.Sp.SecurityData> OpenedSecurities { get; }
    }
}
