using Cherries.Models.dbo;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using Cherries.Services;
using Cherries.Services.Interfaces;
using Cherries.WebApi.Filters;
using Cherries.WebApi.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.SessionState;

namespace Cherries.WebApi.Controllers
{
    [RoutePrefix("api/Optimization")]
    [APISessionValidation]
    public class OptimizationController : BaseController
    {
        private IOptimizationService service;// = new OptimizationService();
        private ISecuritiesService secService;

        public OptimizationController(IOptimizationService opService, ISecuritiesService securitiesService)
        {
            service = opService;
            secService = securitiesService;
        }

        public IHttpActionResult Post(OptimazationExportQuery query)
        {
            var vm = service.Export(query.PortID, user, query.OptimalPort);
            return Ok(vm);
        }

        public IHttpActionResult Get()
        {
            HttpContext context = HttpContext.Current;
            Task.Factory.StartNew(delegate
            {
                WebApiApplication.isComplete = false;
                service.UpdateSecurities(context);
                WebApiApplication.isComplete = true;
            });
            return Ok();
        }
    }
}