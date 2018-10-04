using Cherries.Services.Interfaces;
using Cherries.WebApi.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cherries.WebApi.Controllers
{
    [APIAuthorize]
    [SecuritiesLoadAttribute]
    [APISessionValidation]
    public class PricesController : BaseController
    {
        private ISecuritiesService service;
        public PricesController(ISecuritiesService secService)
        {
            service = secService;
        }

        public IHttpActionResult Get(string secId, string currency)
        {
            var res = service.GetSecurityPrices(secId, currency);
            return Ok(res);

        }

        [HttpGet]
        [Route("GetEndPrices")]
        public IHttpActionResult GetEndPrices(List<string> secsIds, DateTime endDate)
        {
            return Ok();
        }
    }
}
