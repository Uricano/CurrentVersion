using Cherries.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFI.BusinessLogic.Interfaces
{
    public interface ILookupBL : IBaseBL
    {
        LicenceServiceViewModel GetLicenceService();
        CountriesViewModel GetCountries();
        DistrictsViewModel GetDistricts();
        GenderViewModel GetGenders();
        CurrencyViewModel GetCurrencies();
    }
}
