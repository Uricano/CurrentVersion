using Cherries.Models.Command;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFI.BusinessLogic.Interfaces
{
    public interface IUserBL : IBaseBL
    {
        UserViewModel loginToServer(LoginQuery query);
        UserViewModel GetUser(long userId);
        BaseViewModel ChangePassword(ChangePasswordCommand command);
        void Reconnect(ReconnectCommand command);
        void Logoff(LogoffCommand command);
        BaseViewModel CreateUser(SaveUserCommand command);
        BaseViewModel UserExists(String username);
        BaseViewModel UpdateUser(SaveUserCommand command);
        ConfirmCodeViewModel SendConfirmCode(SendConfirmCodeQuery query);
        void SendGreetingsEmail(String name, String email);
    }
}
