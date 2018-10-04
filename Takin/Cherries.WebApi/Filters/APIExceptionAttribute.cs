using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;

namespace Cherries.WebApi.Filters
{
    public class APIExceptionAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            System.Diagnostics.Trace.TraceError("\"" + actionExecutedContext.Exception.ToString() + "\"");
            //actionExecutedContext.Exception = new Exception("Operation Failed. Please Contact System Administrator");
            base.OnException(actionExecutedContext);
        }

        //public override System.Threading.Tasks.Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, System.Threading.CancellationToken cancellationToken)
        //{
        //    System.Diagnostics.Trace.TraceError(actionExecutedContext.Exception.Message + "\n" + actionExecutedContext.Exception.StackTrace);
        //    return base.OnExceptionAsync(actionExecutedContext, cancellationToken);
        //}
    }
}