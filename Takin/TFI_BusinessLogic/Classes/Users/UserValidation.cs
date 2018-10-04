using Cherries.Models.ViewModel;
using Ness.DataAccess.Repository;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TFI.Consts;

namespace TFI.BusinessLogic.Classes.Users
{
    internal class UserValidation
    {
        private readonly IRepository repository;

        public UserValidation(IRepository rep)
        {
            repository = rep;
        }


        public bool UserExists(string username, BaseViewModel vm)
        {
            Entities.dbo.Users user = null;
            repository.Execute(session =>
            {
                user = session.Query<Entities.dbo.Users>().Where(x => x.Username == username).FirstOrDefault();
            });
            if (user != null)
            {
                vm.Messages.Add(new Cherries.Models.App.Message()
                {
                    LogLevel = Cherries.Models.App.LogLevel.Error,
                    Text = Messages.UserExists
                });
                return true;
            }
            return false;

        }

        public bool ValidatePassword(string password, BaseViewModel vm)
        {
            Regex passwordTestExp = new Regex("^(?=.*[0-9])(?=.*[a-zA-Z])([a-zA-Z0-9]+)$");
            if (password.Length < 8 || !passwordTestExp.IsMatch(password))
            {
                vm.Messages.Add(new Cherries.Models.App.Message()
                {
                    LogLevel = Cherries.Models.App.LogLevel.Error,
                    Text = Messages.InvalidPasswordCombination
                });
                return false;
            }
            return true;
        }

        public bool ValidatePayment(double serverSum, double clientSum, BaseViewModel vm)
        {
            if (serverSum != clientSum)
            {
                vm.Messages.Add(new Cherries.Models.App.Message
                {
                    LogLevel = Cherries.Models.App.LogLevel.Error,
                    Text = Messages.PaymentNotMatch
                });
                return false;
            }
            return true;
        }
    }
}
