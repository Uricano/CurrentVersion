using Cherries.Models.dbo;
using Cherries.Models.Queries;
using Cherries.Models.Command;
using Cherries.Models.ViewModel;
using Cherries.Services.Interfaces;
using Cherries.WebApi.Filters;
using Cherries.WebApi.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using TFI.Consts;

namespace Cherries.WebApi.Controllers
{
    [RoutePrefix("api/User")]
    [SecuritiesLoadAttribute]
    public class UserController : ApiController
    {
        IUserService service;
        public UserController(IUserService userService)
        {
            service = userService;
        }
        public HttpResponseMessage Post(LoginQuery query)
        {
            query.IP = HttpContext.Current.Request.UserHostAddress;
            var res = service.Login(query);
            if (res.Messages.Count == 0)
            {
               
                HttpContext.Current.Session.Add("user", res.User);
                HttpContext.Current.Session.Add("ip", HttpContext.Current.Request.UserHostAddress);
                string token = TokenService.GenerateToken(res.User.Username, HttpContext.Current.Request.UserHostAddress, HttpContext.Current.Request.UserAgent, DateTime.Now.Ticks);
                HttpContext.Current.Response.AppendHeader("SessionID", token);
                HttpContext.Current.Response.AppendHeader("Access-Control-Expose-Headers", "SessionID");
                HttpContext.Current.Session.Add("SessionID", token);
                WebApiApplication.lstUsers.Add(res.User.UserID);
            }
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        public HttpResponseMessage Put(ChangePasswordCommand command)
        {
            var res = service.ChangePassword(command);
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        [Route("Logoff")]
        [HttpGet]
        public HttpResponseMessage Logoff()
        {
            if (HttpContext.Current.Session["user"] != null)
            {
                service.Logoff(new LogoffCommand
                {
                    IP = HttpContext.Current.Session["ip"].ToString(),
                    Users = new List<long> { ((User)HttpContext.Current.Session["user"]).UserID }
                });
                WebApiApplication.lstUsers.Remove(((User)HttpContext.Current.Session["user"]).UserID);
                HttpContext.Current.Session.Abandon();
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [Route("Create")]
        [HttpPost]
        public HttpResponseMessage Create(SaveUserCommand command)
        {
            BaseViewModel vm = new BaseViewModel();
            if (HttpContext.Current.Session["PaymentSum"] == null && command.User.Licence.Licensetype != 3)
                vm.Messages.Add(new Models.App.Message
                {
                    LogLevel = Models.App.LogLevel.Error,
                    Text = Messages.CalculationMethodNotUsed
                });
            else
            {
                if (command.User.Licence.Licensetype != 3)
                {
                    command.SumInServer = (double)HttpContext.Current.Session["PaymentSum"];
                    command.User.Licence.Transaction.dSum = (double)HttpContext.Current.Session["PaymentSum"];
                }
                else
                {
                    command.SumInServer = 0;
                    command.User.Licence.Transaction.dSum = 0;
                }
                vm = service.CreateUser(command);
                HttpContext.Current.Session["PaymentSum"] = null;
            }

            if (vm.Messages.Count == 0)
            {
                service.SendGreetingsEmail(command.User.Name, command.User.Email);
            }

            return Request.CreateResponse(HttpStatusCode.OK, vm);
        }

        [Route("SendConfirmCode")]
        [HttpPost]
        public HttpResponseMessage SendConfirmCode(SendConfirmCodeQuery query)
        {
            var codeVM = service.SendConfirmCode(query);
            if (codeVM.Messages.Count == 0)
                HttpContext.Current.Session.Add("ConfirmCode", codeVM.Code);
            codeVM.Code = "";
            return Request.CreateResponse(HttpStatusCode.OK, codeVM);
        }

        [Route("VerifyConfirmCode")]
        [HttpPost]
        public HttpResponseMessage VerifyConfirmCode(VerifyConfirmCodeQuery query)
        {
            BaseViewModel vm = new BaseViewModel();
            var sessionCode = (string)HttpContext.Current.Session["ConfirmCode"] ?? HttpContext.Current.Session["ConfirmCode"].ToString();
            if (sessionCode != query.ConfirmCode)
            {
                vm.Messages.Add(new Models.App.Message
                {
                    LogLevel = Models.App.LogLevel.Error,
                    Text = Messages.ConfirmationCodeInvalid
                });
            }
            return Request.CreateResponse(HttpStatusCode.OK, vm);
        }

        [Route("VerifyUsername")]
        [HttpPost]
        public HttpResponseMessage VerifyUsername(VerifyUsernameCommand command)
        {
            return Request.CreateResponse(HttpStatusCode.OK, service.UserExists(command.Username));
        }

        [Route("Update")]
        [HttpPost]
        public HttpResponseMessage Update(SaveUserCommand query)
        {
            var res = service.UpdateUser(query);
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        
    }
}
