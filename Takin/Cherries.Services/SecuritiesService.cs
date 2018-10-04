using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using Cherries.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFI.BusinessLogic.Interfaces;

namespace Cherries.Services
{
    public class SecuritiesService : ISecuritiesService
    {
        private ISecurities SecuritiesBL;
        private IPriceHandler PricesBL;

        public SecuritiesService(ISecurities securities, IPriceHandler prices)
        {
            SecuritiesBL = securities;
            PricesBL = prices;
        }

        public TopSecuritiesViewModel GetTopSecurities(string currency, List<int> exchanges, List<int> ranks)
        {
            TopSecuritiesViewModel securitiesVM = new TopSecuritiesViewModel();
            securitiesVM.Securities = SecuritiesBL.GetTopSecurities(exchanges, ranks);
            return securitiesVM;
        }

        public BenchMarkSecuritiesViewModel GetBenchmarkSecurities()
        {
            BenchMarkSecuritiesViewModel securitiesVM = new BenchMarkSecuritiesViewModel();
            securitiesVM.Securities = SecuritiesBL.GetBenchmarkSecurities();
            return securitiesVM;
        }

        public SecurityPricesViewModel GetSecurityPrices(string secId, string currency)
        {
            SecurityPricesViewModel vm = new SecurityPricesViewModel();
            vm.Rates = PricesBL.GetPrices(secId, currency);
            return vm;
        }

        public int GetSecurityInMemoryCount()
        {
            return SecuritiesBL.GetSecurityInMemoryCount();
        }

        public TopSecuritiesViewModel GetAllSecurities(AllSecuritiesQuery query)
        {
            TopSecuritiesViewModel securitiesVM = SecuritiesBL.GetAllSecurities(query);
            return securitiesVM; 
        }
    }
}
