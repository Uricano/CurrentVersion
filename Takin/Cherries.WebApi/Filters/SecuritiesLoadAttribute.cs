using Cherries.Models.App;
using Cherries.Models.ViewModel;
using Cherries.TFI.BusinessLogic.Securities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using TFI.BusinessLogic.Interfaces;
using TFI.Consts;

namespace Cherries.WebApi.Filters
{
    public class SecuritiesLoadAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!WebApiApplication.isComplete && StaticData<cSecurity, ISecurities>.lst == null)
            {
                var vm = new BaseViewModel() {
                    Messages = new List<Models.App.Message>() { new Models.App.Message { Text = Messages.SecuritiesAreLoading, LogLevel = Models.App.LogLevel.Error } } };
                actionContext.Response = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.OK, vm);
            }
            else
                base.OnActionExecuting(actionContext);
        }
    }
}