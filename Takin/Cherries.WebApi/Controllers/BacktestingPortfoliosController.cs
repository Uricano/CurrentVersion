using Cherries.Models.Command;
using Cherries.Models.dbo;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
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
using System.Web.Mvc;
using TFI.Consts;

namespace Cherries.WebApi.Controllers
{
    //[APIAuthorize]
    [SecuritiesLoadAttribute]
    [APISessionValidation]
    public class BacktestingPortfoliosController : ApiControllerWithHub<TakinHub>
    {
        private IBacktestingPortfolioService service;// = new PortfolioService();

        public BacktestingPortfoliosController(IBacktestingPortfolioService portService)
        {
            service = portService;
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get([FromUri]GetPortfolioQuery query)
        {
            query.userId = user.UserID;
            BacktestingPortsViewModel res = null;// CacheHelper.Get(base.user.Username + "backtesting") as BacktestingPortsViewModel;
            if (res == null)
            {
                res = service.GetPortolios(query);
                //CacheHelper.Add(base.user.Username + "backtesting", res, DateTime.Now.AddMinutes(30));
            }
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get(int id)
        {
            var res = service.GetFullPortfolio(user, id);
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        public IHttpActionResult Delete(int portId)
        {
            var res = service.DeletePortfolio(user, portId);
            //CacheHelper.Delete(user.Username + "backtesting");
            return Ok(res);
        }


    }//of controller
}