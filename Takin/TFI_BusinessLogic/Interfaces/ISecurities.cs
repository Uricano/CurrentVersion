using Cherries.Models.App;
using Cherries.Models.dbo;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using Cherries.TFI.BusinessLogic.Securities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.Entities.dbo;
using TFI.Entities.Sp;

namespace TFI.BusinessLogic.Interfaces
{
    public interface ISecurities : IBaseBL
    {
        cSecurities getListOfActiveSecs();
        void undisableSecuritiesList();
        List<Cherries.Models.dbo.Security> GetTopSecurities(List<int> exchangesPackagees, List<int> ranks, string security_list = "ALL");
        List<Cherries.Models.dbo.BMsecurity> GetBMSecurities();
        List<Cherries.Models.dbo.BMPrice> GetBMPrices();
        ISecurities GetCustomSecurities(IQueryable<TopSecurities> securities);
        TopSecuritiesViewModel GetAllSecurities(AllSecuritiesQuery query);
        List<GeneralItem> GetBenchmarkSecurities();
        void removeInactiveSecurities(ISecurities cDisabled);
        void sortSecurities();
        int searchSecurity(ISecurity cCurrSec);
        ISecurity getSecurityById(String iSecId);
        ISecurity getSecurityByIdNormalSearch(String iSecId);
        void Add(ISecurity NewSecurity);
        ISecurity this[int Index] { get; }
        int Count { get; }
        void Clear();
        List<ISecurity> Securities { get; set; }

        int GetSecurityInMemoryCount();
    }
}
