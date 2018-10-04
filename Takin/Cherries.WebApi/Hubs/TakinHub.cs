using Cherries.Models.Command;
using Cherries.Models.dbo;
using Cherries.Models.Queries;
using Cherries.Services.Interfaces;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Ness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Cherries.WebApi.Hubs
{
    [HubName("takin")]
    public class TakinHub : Hub
    {

        private IPortfolioService _service;
        private IUserService _userService;

        public TakinHub(IPortfolioService portfolioService, IUserService userService)
        {
            _service = portfolioService;
            _userService = userService;
        }

        public dynamic GetCaller()
        {
            return this.Clients.Caller;
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var userId = WebApiApplication.hubUsers.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
                      LogoffCommand cmd = new LogoffCommand { IP = "", Users = new List<long>() { userId } };
            _userService.Logoff(cmd);
           // WebApiApplication.hubUsers.Remove(userId);
            if (WebApiApplication.hubUsers.ContainsKey(userId))
                this.Clients.Client(WebApiApplication.hubUsers[userId]).disconnected();

            return base.OnDisconnected(stopCalled);
        }

        public void RegisterClient(string userID)
        {

            long user = long.Parse(userID);
            if (WebApiApplication.hubUsers.ContainsKey(user))
            {
                if (Context.ConnectionId == WebApiApplication.hubUsers[user]) return;
                //Clients.Client(WebApiApplication.hubUsers[user]).sessionEnded();
                WebApiApplication.hubUsers.Remove(user);
            }
            WebApiApplication.hubUsers.Add(user, Context.ConnectionId);
            //_userService.Reconnect(new ReconnectCommand { IP = Context.Request.Environment["server.RemoteIpAddress"].ToString(), UserID = user });
        }

        public void CreatePortfolio(string cmdString)
        {
            CreatePortfolioCommand cmd = Newtonsoft.Json.JsonConvert.DeserializeObject<CreatePortfolioCommand>(cmdString);
            Models.ViewModel.OptimalPortoliosViewModel vm = new Models.ViewModel.OptimalPortoliosViewModel();
            string userName = "";
            try
            {
                var userId = WebApiApplication.hubUsers.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
                User user = _userService.GetUser(userId).User;
                userName = user.Username;
                vm = _service.CreatePortfolio(user, cmd);
            }
            catch (Exception e)
            {
                vm.Messages.Add(new Models.App.Message { Text = e.Message, LogLevel = Models.App.LogLevel.Error });
            }
            if (!string.IsNullOrEmpty(userName) && vm.Messages.Count == 0) CacheHelper.Delete(userName + "portfolio");
            Clients.Caller.update(Newtonsoft.Json.JsonConvert.SerializeObject(vm));
        }

    }
}