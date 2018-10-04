using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
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
    [RoutePrefix("api/Securities")]
    //[APIAuthorize]
    [SecuritiesLoadAttribute]
    [APISessionValidation]
    public class SecuritiesController : BaseController
    {
        private ISecuritiesService service;

        public SecuritiesController(ISecuritiesService secService)
        {
            service = secService;
        }

        public IHttpActionResult Get()
        {
            var res = service.GetTopSecurities(user.Currency.CurrencyId, user.Licence.Stocks.Select(t => t.id).ToList(), new List<int>() { 1 });
            return Ok(res);
        }

        [Route("GetBenchmarkSecurities")]
        public IHttpActionResult GetBenchmarkSecurities()
        {
            var res = service.GetBenchmarkSecurities();
            return Ok(res);
        }

        [Route("GetAll")]
        public IHttpActionResult GetAll([FromUri] AllSecuritiesQuery query)
        {
            if (user.Licence.Stocks == null || user.Licence.Stocks.Count == 0)
                return Ok();
            var exchanges = (from s in user.Licence.Stocks
                             join e in query.exchangesPackagees on s.id equals e
                             select s.id).ToList();
            query.exchangesPackagees = exchanges;

            var res = service.GetAllSecurities(query);
            return Ok(res);
        }
    }
}
