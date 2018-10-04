using Cherries.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cherries.WebApi.Controllers
{
    public class LookupController : ApiController
    {
        private ILookupService service;
        public LookupController(ILookupService lookupService)
        {
            service = lookupService;
        }

        public IHttpActionResult Get()
        {
            var res = service.GetLookups();
            return Ok(res);
        }
    }
}
