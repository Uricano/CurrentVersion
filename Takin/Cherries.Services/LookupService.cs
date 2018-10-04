using Cherries.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFI.BusinessLogic.Interfaces;
using Cherries.Models.ViewModel;
using Cherries.TFI.BusinessLogic.Categories;

namespace Cherries.Services
{
    public class LookupService : ILookupService
    {
        private ICategoriesHandler catHandler;
        private ILookupBL _lookupBL;

        public LookupService(ICategoriesHandler categoryHandler, ILookupBL lookupBL)
        {
            catHandler = categoryHandler;
            _lookupBL = lookupBL;
        }

        public LookupViewModel GetLookups()
        {
            LookupViewModel vm = new LookupViewModel();
            foreach (cCatItem item in catHandler.Categories)
            {
                vm.Categories.Add(item.CategoryType.ToString(), item.Items);
            }
            var riskGrid = new TFI.BusinessLogic.StaticMethods.cPortfolioRiskGrid();
            riskGrid.populateInitialCollectionValues();
            vm.PortRisks = riskGrid.PortRisks;
            vm.Services = _lookupBL.GetLicenceService().Services;
            //vm.Countries = _lookupBL.GetCountries().Countries;
            //vm.Districts = _lookupBL.GetDistricts().Districts;
            //vm.Genders = _lookupBL.GetGenders().GenderList;
            vm.Currencies = _lookupBL.GetCurrencies().Currencies;
            return vm;
        }
    }
}
