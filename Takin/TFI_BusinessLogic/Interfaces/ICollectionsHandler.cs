using Cherries.Models.dbo;
using Cherries.TFI.BusinessLogic.Securities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.BusinessLogic.Enums;

namespace TFI.BusinessLogic.Interfaces
{
    public interface ICollectionsHandler : IBaseBL
    {
        void filterSecuritiesForNewPortfolio(List<int> colExchanges);
        void setDisabledSecsToActive();
        void SetSecurites();
        bool loadSecuritiesCollections();
        void clearSecsCalculatedOptData();
        void addMissingSecurities(ISecurities newColl, List<string> missingSecs);
        List<cSecurity> convertListToCSecurity(List<Security> colOrigSecs);
        ICategoryItem getCatItemByID(enumCatType eType, int iId, ICategoryCollection cCollection);
        ISecurities Securities { get; set; }
        ISecurities SecuritiesByRisk { get; set; }
        ISecurities ActiveSecs { get; set; }
        ISecurities Benchmarks { get; set; }
        ISecurities DisabledSecs { get;
            set; }
        ICategoryCollection Sectors { get; set; }
        ICategoryCollection Markets { get; set; }
        ICategoryCollection SecTypes { get; set; }
    }
}
