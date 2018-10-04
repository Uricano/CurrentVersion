using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Data;
using Cherries.Services;
using Cherries.Models.Command;
using Cherries.Services.Interfaces;
using Cherries.Models.ViewModel;
using System.Threading.Tasks;
using Cherries.Models.dbo;
using Cherries.WebApi.Hubs;
using Cherries.WebApi.Filters;
using TFI.Consts;
using Ness.Utils;
using Cherries.Models.Queries;

namespace Cherries.WebApi.Controllers
{
    [APIAuthorize]
    [SecuritiesLoadAttribute]
    [APISessionValidation]
    public class PortfoliosController : ApiControllerWithHub<TakinHub>
    {
        private IPortfolioService service;// = new PortfolioService();

        public PortfoliosController(IPortfolioService portService)
        {
            service = portService;
        }

        [HttpGet]
        public HttpResponseMessage Get([FromUri]GetPortfolioQuery query)
        {
            query.userId = user.UserID;
            TopPortfoliosViewModel res = null;  //CacheHelper.Get(base.user.Username + "portfolio") as PortfolioViewModel;
            if (res == null)
            {
                res = service.GetPortolios(query);
                //CacheHelper.Add(base.user.Username + "portfolio", res, DateTime.Now.AddMinutes(30));
            }
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        [HttpGet]
        public HttpResponseMessage Get(int id)
        {
            var res = service.GetFullPortfolio(user, id);
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        public IHttpActionResult Post(CreatePortfolioCommand cmd)
        {
            if (!WebApiApplication.isComplete) return Ok(new BaseViewModel() { Messages = new List<Models.App.Message>() { new Models.App.Message { Text = Messages.SecuritiesAreLoading, LogLevel = Models.App.LogLevel.Info } } });
            if (user.Licence.ExpiryDate < DateTime.Today) return Ok(new BaseViewModel { Messages = new List<Models.App.Message>() { new Models.App.Message { Text = Messages.LicenseExpired, LogLevel = Models.App.LogLevel.Error } } });
            //var res = service.CreatePortfolio(user, cmd);
            //return Ok(res);
            HttpContext context = HttpContext.Current;
            var vm = service.IsMaxPortfolioExceeded(user);
            vm.Messages.AddRange(service.IsPortfolioExists(cmd.Name, user.UserID).Messages);
            if (vm.Messages.Count == 0)
            {
                Task t = new Task(delegate { GetAsyncResponse(user, cmd, context); });
                t.Start();
                return Ok(new { Message = "Your portfolio is being created, you will be notified when the portfolio is ready." });
            }
            return Ok(new { Message = vm.Messages[0].Text });
        }

        public IHttpActionResult Put(UpdatePortfolioCommand cmd)
        {
            var res = service.UpdatePortfolio(user, cmd);
            return Ok(res);
        }

        public IHttpActionResult Delete(int portId)
        {
            var res = service.DeletePortfolio(user, portId);
            //CacheHelper.Delete(user.Username + "portfolio");
            return Ok(res);
        }

        private void GetAsyncResponse(User user, CreatePortfolioCommand cmd, HttpContext context)
        {
            HttpContext.Current = new HttpContext(context.Request, context.Response);

            service = WebApiApplication.Container.Resolve<IPortfolioService>();
            var vm = service.CreatePortfolio(user, cmd);
            WebApiApplication.Container.Release(service);
            //CacheHelper.Delete(user.Username + "portfolio");
            this.Hub.Clients.Client(WebApiApplication.hubUsers[user.UserID]).update(Newtonsoft.Json.JsonConvert.SerializeObject(vm));
        }
    }
}
