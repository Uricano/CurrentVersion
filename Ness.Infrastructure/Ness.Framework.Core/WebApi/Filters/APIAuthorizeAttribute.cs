using Framework.Core.Utils;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Ness.Framework.Core.WebApi.Filters
{
    public class APIAuthorizeAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {

            HttpRequestMessage message = actionContext.Request;
            //message.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "IdentityNumber", "123456789"))));

            var authHeader = actionContext.Request.Headers.Authorization;

            if (authHeader != null)
            {
                if (authHeader.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(authHeader.Parameter))
                {
                    var rawCredentials = authHeader.Parameter;
                    // var encoding = Encoding.GetEncoding("iso-8859-1");
                    // var credentials = encoding.GetString(Convert.FromBase64String(rawCredentials));

                    if (!TokenService.IsTokenValid(rawCredentials, HttpContext.Current.Request.UserHostAddress, HttpContext.Current.Request.UserAgent))
                        HandleUnAuthorized(actionContext);

                    return;
                }
            }

            HandleUnAuthorized(actionContext);
        }

        private void HandleUnAuthorized(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
        }
    }
}