using Cherries.Models.dbo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Cherries.WebApi.Controllers
{
    public class BaseController : ApiController
    {
        protected User user { get; set; }

        public BaseController()
        {
            user = HttpContext.Current.Session["user"] as User;
            
        }
    }
}
