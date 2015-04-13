using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Rest.Controllers
{
    [RoutePrefix("api/schema/v1")]
    public class ContentTypeApiController : UmbracoApiController
    {
        [Route("")]
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}