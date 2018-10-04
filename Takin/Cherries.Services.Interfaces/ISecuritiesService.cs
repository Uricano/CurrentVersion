using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cherries.Services.Interfaces
{
    public interface ISecuritiesService : IServiceBase
    {
        TopSecuritiesViewModel GetTopSecurities(string currency, List<int> exchanges, List<int> ranks);
        SecurityPricesViewModel GetSecurityPrices(string secId, string currency);
        BenchMarkSecuritiesViewModel GetBenchmarkSecurities();
        int GetSecurityInMemoryCount();
        TopSecuritiesViewModel GetAllSecurities(AllSecuritiesQuery query);
    }
}
