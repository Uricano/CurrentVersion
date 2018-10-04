using Cherries.Models.App;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.Securities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.BusinessLogic.Interfaces
{
    public interface IRateHandler : IBaseBL
    {
        //void calcSecurityRates(ISecurities cSecsCol, cDateRange drPeriod, Boolean isRemoveDisabled);
        List<PriceReturn> setSecuritiesPriceReturns(ISecurities cSecsCol, DateTime fromDT, DateTime toDT, string calcCurrency);
        void Dispose();
    }
}
