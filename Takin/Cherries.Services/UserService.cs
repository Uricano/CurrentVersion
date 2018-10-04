using Cherries.Models.Command;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using Cherries.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFI.BusinessLogic.Classes.Users;
using TFI.BusinessLogic.Interfaces;

namespace Cherries.Services
{
    public class UserService : IUserService
    {
        private IUserBL userBL;

        public UserService(IUserBL uService)
        {
            userBL = uService;
        }
        public UserViewModel Login(LoginQuery query)
        {
            
            UserViewModel vm = new UserViewModel();
            vm = userBL.loginToServer(query);
            return vm;
        }

        public UserViewModel GetUser(long userId)
        {
            UserViewModel vm = new UserViewModel();
            vm = userBL.GetUser(userId);
            return vm;
        }

        public BaseViewModel ChangePassword(ChangePasswordCommand command)
        {
            BaseViewModel vm = new BaseViewModel();
            vm = userBL.ChangePassword(command);
            return vm;
        }
        public void Reconnect(ReconnectCommand command)
        {
            userBL.Reconnect(command);
        }
        public void Logoff(LogoffCommand command)
        {
            userBL.Logoff(command);
        }

        public BaseViewModel CreateUser(SaveUserCommand command)
        {
            BaseViewModel vm = new BaseViewModel();
            vm = userBL.CreateUser(command);
            return vm;
        }

        public BaseViewModel UserExists(String username)
        {
            return userBL.UserExists(username);
        }

        public ConfirmCodeViewModel SendConfirmCode(SendConfirmCodeQuery query)
        {
            ConfirmCodeViewModel code = userBL.SendConfirmCode(query);
            return code;
        }

        public BaseViewModel UpdateUser(SaveUserCommand command)
        {
            BaseViewModel vm = new BaseViewModel();
            vm = userBL.UpdateUser(command);
            return vm;
        }
        
        public void SendGreetingsEmail(String name, String email)
        {
            userBL.SendGreetingsEmail(name, email);
        }
    }
}
