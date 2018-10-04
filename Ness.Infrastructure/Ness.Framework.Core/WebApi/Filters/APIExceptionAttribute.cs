using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;

namespace Ness.Framework.Core.WebApi.Filters
{
    public class APIExceptionAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            System.Diagnostics.Trace.TraceError("\"" + actionExecutedContext.Exception.ToString() + "\"");
            actionExecutedContext.Exception = new Exception("תקלת מערכת, אנא פנה למנהל מערכת");
            base.OnException(actionExecutedContext);
        }
    }
}