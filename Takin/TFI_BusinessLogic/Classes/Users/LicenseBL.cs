using Cherries.Models.Command;
using Cherries.Models.ViewModel;
using Ness.DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFI.BusinessLogic.Interfaces;
using Cherries.Models.Lookup;
using TFI.Entities.dbo;
using Cherries.Models.Queries;
using TFI.Consts;
using Cherries.Models.dbo;
using NHibernate.Linq;

namespace TFI.BusinessLogic.Classes.Users
{
    public class LicenseBL : ILicenseBL
    {
        #region Data members

        // Main variables
        private IErrorHandler m_objErrorHandler;// = new cErrorHandler(); // Error handler
        private IRepository repository; //= new Repository();
        private UserValidation validator;
       
        #endregion Data members

        public LicenseBL(IErrorHandler error, IRepository rep)
        {
            m_objErrorHandler = error;
            repository = rep;
            validator = new UserValidation(repository);
        }
        public LicenseViewModel UpdateLicense(UpdateLicenseCommand command)
        {
            LicenseViewModel vm = new LicenseViewModel();

            if (validator.ValidatePayment(command.SumInServer, command.Transaction.dSum, vm))
            {
                Dictionary<string, Tuple<object, NHibernate.Type.IType>> param = new Dictionary<string, Tuple<object, NHibernate.Type.IType>>();
                param.Add("@LicenseID", new Tuple<object, NHibernate.Type.IType>(command.LicenseID, NHibernate.NHibernateUtil.Int32));
                repository.ExecuteSp("DeleteLicenseStocks", param);
                repository.ExecuteTransaction(session =>
                {
                    var license = session.Get<Entities.dbo.Userlicenses>(command.LicenseID);
                    var transaction = AutoMapper.Mapper.Map<Entities.dbo.LicTransactions>(command.Transaction);
                    session.Save(transaction);
                    var user = session.Get<Entities.dbo.Users>(command.UserID);
                    license.User = user;
                    license.Transaction = transaction;
                    license.tb_LicServices = new Licservices { Idlicservice = command.Idlicservice };
                    var service = session.Get<Entities.dbo.Licservices>(command.Idlicservice);
                    license.dtExpirationDate = DateTime.Today.AddMonths(service.Imonths);
                    license.dtActivationDate = DateTime.Today;
                    license.dtPurchaseDate = DateTime.Today;
                    session.Update(license);
                    license.Licenseexchanges = new List<Entities.Lookup.Licenseexchanges>();

                    AddExchanges(license, command.Stocks.ToList());
                    session.Update(license);


                });
                repository.Execute(session =>
                {
                    var licenseUpd = session.Query<Entities.dbo.Userlicenses>().Where(x => x.User.UserID == command.UserID).FirstOrDefault();
                    vm.License = AutoMapper.Mapper.Map<UserLicence>(licenseUpd);
                });
            }
            return vm;
        }

        public LicenseCalculationViewModel LicenseCalculation(LicenseCalculationQuery query, User user)
        {
            LicenseCalculationViewModel vm = new LicenseCalculationViewModel();

            repository.Execute(session =>
            {
                double refund = 0;
                var service = session.Get<Licservices>(query.ServiceID);
                refund = GetTotalRefund(user);
                vm.Sum = service.Dstartprice + ((query.StockCount - service.Ibaseexchanges) * service.Dnewexchangeprice) - refund;
                vm.Sum = vm.Sum < 0 ? 0 : vm.Sum;
            });

            return vm;
        }

        private double GetTotalRefund(User user)
        {
            double refund = 0;
            if (user != null)
            {
                var total = user.Licence.Transaction != null ? user.Licence.Transaction.dSum : 0;
                var pricePerDay = total / (user.Licence.ExpiryDate.Subtract(user.Licence.ActivationDate).TotalDays);
                refund = ((user.Licence.ExpiryDate.Subtract(DateTime.Today)).TotalDays) * pricePerDay;
            }
            return refund > 0 ? refund : 0;
        }

        private void AddExchanges(Userlicenses license, List<StockMarket> stocks)
        {
            foreach (var item in stocks)
            {
                license.Licenseexchanges.Add(new Entities.Lookup.Licenseexchanges
                {
                    Userlicenses = new Entities.dbo.Userlicenses { LicenseID = license.LicenseID },
                    Stockexchanges = AutoMapper.Mapper.Map<Entities.Lookup.SelStockExchange>(item)

                });
            }
        }
    }
}
