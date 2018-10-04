using Cherries.Models.dbo;
using Cherries.Models.ViewModel;
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Protection.LicenseManagement;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TFI.BusinessLogic.Interfaces;
using Ness.DataAccess.Repository;
using Ness.Utils;
using Cherries.Models.Queries;
using System.Text.RegularExpressions;
using Cherries.Models.Command;
using TFI.Consts;

namespace TFI.BusinessLogic.Classes.Users
{
    public class cUser : IUserBL
    {
        #region Data members

        // Main variables
        private IErrorHandler m_objErrorHandler;// = new cErrorHandler(); // Error handler
        private IRepository repository; //= new Repository();
        private UserValidation validator;
        private const string _Cupon = "tak1155449977";
        private const string _CuponIsrael = "tak1187649137";


        #endregion Data members

        public cUser(IErrorHandler error, IRepository rep)
        {
            m_objErrorHandler = error;
            repository = rep;
            validator = new UserValidation(rep);
        }

        #region Login Methods

        public UserViewModel loginToServer(LoginQuery query)
        { // Attempts to login to remote server
            UserViewModel vm = new UserViewModel();
            repository.ExecuteTransaction(session =>
            {
                var res = session.Query<TFI.Entities.dbo.Userlicenses>().Where
                (x => x.User.Username == query.Username || x.User.Email.ToLower() == query.Username.ToLower()).OrderByDescending(x => x.dtExpirationDate).FirstOrDefault();
                if (res == null || !EncryptionHelper.ValidateHashData(query.Password, res.User.Password, res.User.Salt, EncryptionHelper.encriptionType.MD5))
                {
                    vm.Messages.Add(new Cherries.Models.App.Message()
                    {
                        LogLevel = Cherries.Models.App.LogLevel.Error,
                        Text = Messages.UserOrPasswordInvalid
                    });
                }
                //else if (res.dtExpirationDate < DateTime.Today)
                //{
                //    vm.Messages.Add(new Cherries.Models.App.Message()
                //    {
                //        LogLevel = Cherries.Models.App.LogLevel.Error,
                //        Text = Messages.LicenseExpired
                //    });
                //}
                else if (res.User.IsLoggedin)
                    vm.Messages.Add(new Cherries.Models.App.Message()
                    {
                        LogLevel = Cherries.Models.App.LogLevel.Error,
                        Text = Messages.UserAlreayLoggedin
                    });
                else
                {
                    res.User.IsLoggedin = true;
                    res.User.LoggedinIP = query.IP;
                    session.Update(res.User);

                    Entities.dbo.UsersLogin usersLogin = new Entities.dbo.UsersLogin
                    {
                        UserID = res.User.UserID,
                        LoginDT = DateTime.Now,
                        LogoutDT = null
                    };
                    session.Save(usersLogin);
                    session.Flush();
                    vm.User = AutoMapper.Mapper.Map<User>(res);

                }
            });
            return vm;
        }//loginToServer

        public UserViewModel GetUser(long userId)
        {
            UserViewModel vm = new UserViewModel();
            repository.ExecuteTransaction(session =>
            {
                var res = session.Query<Entities.dbo.Userlicenses>().Where(x => x.User.UserID == userId).FirstOrDefault();
                vm.User = AutoMapper.Mapper.Map<User>(res);
            });
            return vm;
        }
        public void Logoff(LogoffCommand command)
        {
            repository.ExecuteTransaction(session =>
            {
                foreach (var userID in command.Users)
                {
                    var user = session.Get<Entities.dbo.Users>(userID);
                    user.IsLoggedin = false;
                    user.LoggedinIP = "";
                    session.Update(user);

                    Entities.dbo.UsersLogin usersLogin = session.Query<Entities.dbo.UsersLogin>().Where(x => (x.UserID == userID && x.LogoutDT == null)).FirstOrDefault();
                    if (usersLogin != null)
                    {
                        usersLogin.LogoutDT = DateTime.Now;
                        session.Update(usersLogin);
                    }
                }
            });
        }
        public void Reconnect(ReconnectCommand command)
        {
            repository.ExecuteTransaction(session =>
            {
                var user = session.Get<Entities.dbo.Users>(command.UserID);
                user.LoggedinIP = command.IP;
                user.IsLoggedin = true;
                session.Update(user);
            });
        }
        public BaseViewModel CreateUser(SaveUserCommand command)
        {
            BaseViewModel vm = new BaseViewModel();
            Entities.dbo.Users newUser = null;
            Entities.dbo.LicTransactions trans = null;
            Entities.dbo.Userlicenses userLic = null;
            if (command.User.Licence.Licensetype == 3)
            {
                if ((command.Cupon != _Cupon) && (command.Cupon != _CuponIsrael))
                {
                    vm.Messages.Add(new Cherries.Models.App.Message { LogLevel = Cherries.Models.App.LogLevel.Error, Text = "Invalid coupon code" });
                    return vm;
                }
            }
            if (validator.ValidatePassword(command.Password, vm) && !validator.UserExists(command.User.Username,vm) && validator.ValidatePayment(command.SumInServer, command.User.Licence.Transaction.dSum, vm))
            {
                repository.ExecuteTransaction(session =>
                {
                    var user = session.Query<Entities.dbo.Users>().Where(x => x.Username == command.User.Username).FirstOrDefault();
                    if (command.User.Currency == null) command.User.Currency = new Cherries.Models.Lookup.Currency { CurrencyId = "9001" };
                    newUser = AutoMapper.Mapper.Map<Entities.dbo.Users>(command.User);
                    userLic = newUser.Userlicenses[0];
                    newUser.Userlicenses = null;
                    SetPassword(newUser, command.Password, false);
                    session.Save(newUser);
                    trans = userLic.Transaction;
                    session.Save(trans);
                    userLic.User = newUser;
                    userLic.Licensetypes = new Entities.Lookup.Licensetypes { Idlicensetype = command.User.Licence.Licensetype };
                    userLic.Transaction = trans;
                    if (userLic.isTrial)
                        userLic.dtExpirationDate = DateTime.Today.AddDays(90);
                    else
                        userLic.dtExpirationDate = DateTime.Today.AddMonths(userLic.tb_LicServices.Imonths);
                    userLic.dtActivationDate = DateTime.Today;
                    userLic.dtPurchaseDate = DateTime.Today;
                    session.SaveOrUpdate(userLic);
                    userLic.Licenseexchanges = new List<Entities.Lookup.Licenseexchanges>();

                    List<Cherries.Models.Lookup.StockMarket> finalExchanges = getListOfExchangesByCoupon(command.User.Licence.Stocks.ToList(), command);
                    AddExchanges(userLic, finalExchanges);
                    //AddExchanges(userLic, command.User.Licence.Stocks.ToList());

                    session.SaveOrUpdate(userLic);
                });
            }
            return vm;
        }

        private List<Cherries.Models.Lookup.StockMarket> getListOfExchangesByCoupon(List<Cherries.Models.Lookup.StockMarket> colExchanges, SaveUserCommand command)
        {
            List<Cherries.Models.Lookup.StockMarket> colFinalExchanges = new List<Cherries.Models.Lookup.StockMarket>();
            if (command.Cupon == "tak1155449977") // USA
                for (int iRows = 0; iRows < colExchanges.Count; iRows++)
                    if (colExchanges[iRows].id != 1)
                        colFinalExchanges.Add(colExchanges[iRows]);

            if (command.Cupon == "tak1187649137") // ISRAEL
                for (int iRows = 0; iRows < colExchanges.Count; iRows++)
                    if (colExchanges[iRows].id == 1)
                        colFinalExchanges.Add(colExchanges[iRows]);

            if (colFinalExchanges.Count == 0) return colExchanges;
            return colFinalExchanges;
        }//getListOfExchangesByCoupon

        public BaseViewModel createNewUser(SaveUserCommand command)
        { // Creates a new user instance and saves to DB (from registration page)
            BaseViewModel vmFinal = new BaseViewModel();
            Entities.dbo.Users newUser = null;
            Entities.dbo.LicTransactions trans = null;
            Entities.dbo.Userlicenses userLic = null;
            if (command.User.Licence.Licensetype == 3)
                if (command.Cupon != _Cupon)
                { // Invalid coupon
                    vmFinal.Messages.Add(new Cherries.Models.App.Message { LogLevel = Cherries.Models.App.LogLevel.Error, Text = "Invalid coupon code" });
                    return vmFinal;
                }

            // Save user to DB
            if (validator.ValidatePassword(command.Password, vmFinal) && !validator.UserExists(command.User.Username, vmFinal) && validator.ValidatePayment(command.SumInServer, command.User.Licence.Transaction.dSum, vmFinal))
            { // verifies user doesn't already exist + parameters inserted are legal
                repository.ExecuteTransaction(session =>
                {
                    if (command.User.Currency == null)
                        command.User.Currency = new Cherries.Models.Lookup.Currency { CurrencyId = "9001" };

                    newUser = AutoMapper.Mapper.Map<Entities.dbo.Users>(command.User);
                    userLic = newUser.Userlicenses[0];



                    
                    newUser.Userlicenses = null;
                    SetPassword(newUser, command.Password, false);
                    session.Save(newUser);
                    trans = userLic.Transaction;
                    session.Save(trans);
                    userLic.User = newUser;
                    userLic.Licensetypes = new Entities.Lookup.Licensetypes { Idlicensetype = command.User.Licence.Licensetype };
                    userLic.Transaction = trans;
                    if (userLic.isTrial)
                        userLic.dtExpirationDate = DateTime.Today.AddDays(90);
                    else
                        userLic.dtExpirationDate = DateTime.Today.AddMonths(userLic.tb_LicServices.Imonths);
                    userLic.dtActivationDate = DateTime.Today;
                    userLic.dtPurchaseDate = DateTime.Today;
                    session.SaveOrUpdate(userLic);
                    userLic.Licenseexchanges = new List<Entities.Lookup.Licenseexchanges>();
                    AddExchanges(userLic, command.User.Licence.Stocks.ToList());

                    session.SaveOrUpdate(userLic);
                });
            }
            return vmFinal;



        }//createNewUser

        public BaseViewModel UserExists(String username)
        {
            BaseViewModel vm = new BaseViewModel();
            validator.UserExists(username, vm);
            return vm;
        }

        public BaseViewModel UpdateUser(SaveUserCommand command)
        {
            BaseViewModel vm = new BaseViewModel();
            if (!validator.UserExists(command.User.Username, vm))
            {
                vm.Messages.Add(new Cherries.Models.App.Message
                {
                    LogLevel = Cherries.Models.App.LogLevel.Error,
                    Text = Messages.UserNotExist
                });

            }
            else
            {
                vm.Messages.Clear();
                repository.ExecuteTransaction(session =>
                {
                    var user = session.Get<Entities.dbo.Users>(command.User.UserID);
                    user.Name = command.User.Name;
                    user.CellPhone = command.User.CellPhone;
                    user.Email = command.User.Email;
                    //user.Currency = AutoMapper.Mapper.Map <Entities.Lookup.SelCurrency>(command.User.Currency);
                    session.Update(user);
                });
            }
            return vm;
        }
        public BaseViewModel ChangePassword(ChangePasswordCommand command)
        {
            BaseViewModel vm = new BaseViewModel();
            repository.ExecuteTransaction(session =>
            {
                var user = session.Query<TFI.Entities.dbo.Userlicenses>().Where
                     (x => x.User.Email == command.Email).OrderByDescending(x => x.dtExpirationDate).FirstOrDefault();
                if (user == null)
                {
                    vm.Messages.Add(new Cherries.Models.App.Message()
                    {
                        LogLevel = Cherries.Models.App.LogLevel.Error,
                        Text = Messages.DetailsInvalidEmail
                    });
                    return;
                }
                else
                {
                    string password = "";
                    bool isTempPassword;
                    if (string.IsNullOrEmpty(command.OldPassword) && string.IsNullOrEmpty(command.NewPassword))
                    {
                        password = System.IO.Path.GetRandomFileName().Replace(".", "").Substring(0, 8);
                        isTempPassword = true;
                    }
                    else
                    {
                        var encrypted = EncryptionHelper.GetSaltHashData(command.OldPassword.Trim(), user.User.Salt);
                        if (!EncryptionHelper.ValidateHashData(command.OldPassword.Trim(), user.User.Password, user.User.Salt, EncryptionHelper.encriptionType.MD5) || user.User.Email.ToLower() != command.Email.ToLower())
                        {
                            vm.Messages.Add(new Cherries.Models.App.Message()
                            {
                                LogLevel = Cherries.Models.App.LogLevel.Error,
                                Text = Messages.DetailsInvalidPassword
                            });
                            return;
                        }
                        //else if (user.dtExpirationDate < DateTime.Today)
                        //{
                        //    vm.Messages.Add(new Cherries.Models.App.Message()
                        //    {
                        //        LogLevel = Cherries.Models.App.LogLevel.Error,
                        //        Text = Messages.LicenseExpired
                        //    });
                        //    return;
                        //}
                        else if (!validator.ValidatePassword(command.NewPassword.Trim(), vm))
                            return;
                        password = command.NewPassword.Trim();
                        isTempPassword = false;
                    }
                    SetPassword(user.User, password, isTempPassword);
                    //session.Update(user.User);
                    if (isTempPassword)
                    {
                        EmailHandler email = new EmailHandler(true);
                        email.Send(new List<string> { user.User.Email }, "Cherries - Temporary Password", GetTemporaryPasswordMail(user.User.Username, password));
                    }
                }
            });
            return vm;
        }

        public ConfirmCodeViewModel SendConfirmCode(SendConfirmCodeQuery query)
        {
            ConfirmCodeViewModel vm = new ConfirmCodeViewModel();
            if (!validator.UserExists(query.UserName, vm))
            {
                string code = System.IO.Path.GetRandomFileName().Replace(".", "").Substring(0, 8);
                vm.Code = code;
                EmailHandler emailHandler = new EmailHandler(true);
                emailHandler.Send(new List<string> { query.Email }, "Cherries - User Confirmation", GetConfirmationCodeMail(code, query.UserName));
            }
            
            return vm;
        }

        public void SendGreetingsEmail(String name, String email)
        {
            EmailHandler emailHandler = new EmailHandler(false);
            emailHandler.SendGreetingsEmail(name, email);
        }

        private void SetPassword(Entities.dbo.Users user, string password, bool isTempPassword)
        {
            var encrypted = EncryptionHelper.GetSaltHashData(password);
            user.Password = encrypted.Item2;
            user.Salt = encrypted.Item1;
            user.isTemporary = isTempPassword;
        }
        
        private void AddExchanges(Entities.dbo.Userlicenses userLic, List<Cherries.Models.Lookup.StockMarket> stocks)
        {
            foreach (var item in stocks)
            {
                userLic.Licenseexchanges.Add(new Entities.Lookup.Licenseexchanges
                {
                    Userlicenses = new Entities.dbo.Userlicenses { LicenseID = userLic.LicenseID },
                    Stockexchanges = AutoMapper.Mapper.Map<Entities.Lookup.SelStockExchange>(item)

                });
            }
        }

        private string GetTemporaryPasswordMail(string userName, string password)
        {
            string message = $@"<p style=""text-align:left;"">Hello {userName},
                                <br>
                                <br>
                                Your Temporary Password is: {password}
                                <br>
                                <br>
                                Regards,<br>
                                Cherries System
                            </p>";

            return message;
        }

        private string GetConfirmationCodeMail(string code, string userName)
        {
            string message = $@"<p style=""text-align:left;"">Hello {userName},
                                <br>
                                <br>
                                Your confirmation code is: {code}
                                <br>
                                <br>
                                Regards,<br>
                                Cherries System
                            </p>";

            return message;
        }

        #endregion Login Methods
    }
}
