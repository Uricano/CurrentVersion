using Cherries.Models.ViewModel;
using Ness.DataAccess.Repository;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFI.BusinessLogic.Interfaces;

namespace TFI.BusinessLogic.Classes.General
{
    public class LookupBL : ILookupBL
    {
        private IErrorHandler m_objErrorHandler;// = new cErrorHandler(); // Error handler
        private IRepository repository; //= new Repository();

        public LookupBL(IErrorHandler error, IRepository rep)
        {
            m_objErrorHandler = error;
            repository = rep;
        }

        public LicenceServiceViewModel GetLicenceService()
        {
            LicenceServiceViewModel vm = new LicenceServiceViewModel();
            repository.Execute(session =>
            {
                var services = session.Query<Entities.dbo.Licservices>().ToList();
                vm.Services = AutoMapper.Mapper.Map<List<Cherries.Models.dbo.LicenceService>>(services);
            });
            return vm;
        }

        public CountriesViewModel GetCountries()
        {
            CountriesViewModel vm = new CountriesViewModel();
            repository.Execute(session =>
            {
                var contries = session.Query<Entities.Lookup.Country>().ToList();
                vm.Countries = AutoMapper.Mapper.Map<List<Cherries.Models.Lookup.Country>>(contries);
            });
            return vm;
        }

        public DistrictsViewModel GetDistricts()
        {
            DistrictsViewModel vm = new DistrictsViewModel();
            repository.Execute(session =>
            {
                var districts = session.Query<Entities.Lookup.District>().ToList();
                vm.Districts = AutoMapper.Mapper.Map<List<Cherries.Models.Lookup.District>>(districts);
            });
            return vm;
        }

        public GenderViewModel GetGenders()
        {
            GenderViewModel vm = new GenderViewModel();
            repository.Execute(session =>
            {
                var gedners = session.Query<Entities.Lookup.Gender>().ToList();
                vm.GenderList = AutoMapper.Mapper.Map<List<Cherries.Models.Lookup.Gender>>(gedners);
            });
            return vm;
        }

        public CurrencyViewModel GetCurrencies()
        {
            CurrencyViewModel vm = new CurrencyViewModel();
            repository.Execute(session =>
            {
                var currencies = session.Query<Entities.Lookup.SelCurrency>().Where(x => x.IsActive == true).ToList();
                vm.Currencies = AutoMapper.Mapper.Map<List<Cherries.Models.Lookup.Currency>>(currencies);
            });
            return vm;
        }
    }
}
