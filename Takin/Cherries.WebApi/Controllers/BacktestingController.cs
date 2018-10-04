using Cherries.Models.Command;
using Cherries.Models.dbo;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using Cherries.Services;
using Cherries.Services.Interfaces;
using Cherries.WebApi.Filters;
using Cherries.WebApi.Hubs;
using Ness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using TFI.BusinessLogic.Enums;
using TFI.Consts;

namespace Cherries.WebApi.Controllers
{
    //[APIAuthorize]
    [SecuritiesLoadAttribute]
    [APISessionValidation]
    public class BacktestingController : ApiControllerWithHub<TakinHub>
    {
        private IBackTestingService service;// = new BackTestingService();

        public BacktestingController(IBackTestingService backService)
        {
            service = backService;
        }
        public IHttpActionResult Post([FromBody] BackTestingQuery query)
        {
            if (!WebApiApplication.isComplete) return Ok(new BaseViewModel() { Messages = new List<Models.App.Message>() { new Models.App.Message { Text = Messages.SecuritiesAreLoading, LogLevel = Models.App.LogLevel.Info } } });
            if (user.Licence.ExpiryDate < DateTime.Today) return Ok(new BaseViewModel { Messages = new List<Models.App.Message>() { new Models.App.Message { Text = Messages.LicenseExpired, LogLevel = Models.App.LogLevel.Error } } });
            //var res = service.CreatePortfolio(user, cmd);
            //return Ok(res);
            HttpContext context = HttpContext.Current;
            var vm = service.IsPortfolioExists(query.PortfolioCommand.Name, user.UserID);
          //  vm.Messages.AddRange(service.IsPortfolioExists(cmd.Name, user.UserID).Messages);
            if (vm.Messages.Count == 0)
            {
                Task t = new Task(delegate { GetAsyncResponse(user, query, context); });
                t.Start();
                return Ok(new { Message = "Your backtesting portfolio is being created, you will be notified when the portfolio is ready." });
            }
            return Ok(new { Message = vm.Messages[0].Text });
        }

        [HttpGet]
        public HttpResponseMessage Get(int id)
        {
            var res = service.GetBacktestingPortfolio(user, id);
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        private void GetAsyncResponse(User user, BackTestingQuery  query, HttpContext context)
        {

            if (query.PortfolioCommand.CalcType == enumEfCalculationType.Custom && query.PortfolioCommand.Exchanges.Count < 1)
            {
                for (int i = 0; i < user.Licence.Stocks.Count; i++)
                {
                    query.PortfolioCommand.Exchanges.Add(Convert.ToInt32(user.Licence.Stocks[i].id));
                }

                //query.PortfolioCommand.Exchanges = (List<int>)user.Licence.Stocks;
            }

            HttpContext.Current = new HttpContext(context.Request, context.Response);
            service = WebApiApplication.Container.Resolve<IBackTestingService>();
            var vm = service.calculateBacktesting(user, query.PortfolioCommand, query.StartDate, query.EndDate, query.BenchMarkID);
            WebApiApplication.Container.Release(service);
            var a = Newtonsoft.Json.JsonConvert.SerializeObject(vm);
            //CacheHelper.Delete(user.Username + "backtesting");
            this.Hub.Clients.Client(WebApiApplication.hubUsers[user.UserID]).update(a);
        }
    }
}
