using Cherries.Models.Command;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using Cherries.Services.Interfaces;
using Cherries.WebApi.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using TFI.Consts;

namespace Cherries.WebApi.Controllers
{
    [RoutePrefix("api/License")]
    
    [SecuritiesLoadAttribute]
    [APISessionValidation]
    public class LicenseController : BaseController
    {
        private readonly ILicenseService service;
        public LicenseController(ILicenseService licenseSerice)
        {
            service = licenseSerice;
        }

        [Route("Calculate")]
        public IHttpActionResult Get([FromUri]LicenseCalculationQuery query)
        {
            var vm = service.LicenseCalculation(query, user);
            HttpContext.Current.Session.Add("PaymentSum", vm.Sum);
            return Ok(vm);
        }
        [APIAuthorize]
        public IHttpActionResult Post(UpdateLicenseCommand command)
        {
            LicenseViewModel vm = new LicenseViewModel();
            if (HttpContext.Current.Session["PaymentSum"] == null)
                vm.Messages.Add(new Models.App.Message
                {
                    LogLevel = Models.App.LogLevel.Error,
                    Text = Messages.CalculationMethodNotUsed
                });
            else
            {
                command.SumInServer = (double)HttpContext.Current.Session["PaymentSum"];
                command.Transaction.dSum = (double)HttpContext.Current.Session["PaymentSum"];
                vm = service.UpdateLicense(command);
                HttpContext.Current.Session["PaymentSum"] = null;
                user.Licence = vm.License;
                HttpContext.Current.Session.Add("user", user);
            }
            return Ok(vm);
        }
    }
}
