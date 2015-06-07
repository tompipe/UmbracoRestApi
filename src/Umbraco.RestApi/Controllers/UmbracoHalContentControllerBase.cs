using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.RestApi.Links;
using Umbraco.RestApi.Models;
using Umbraco.RestApi.Routing;
using Umbraco.Web;

namespace Umbraco.RestApi.Controllers
{
    public abstract class UmbracoHalContentControllerBase<TId, TEntity, TRepresentation> : UmbracoHalController<TId, TEntity, TRepresentation, IContentLinkTemplate> 
        where TEntity : class
        where TId : struct 
        where TRepresentation : UmbracoRepresentationBase     
    {
        protected UmbracoHalContentControllerBase()
        {
        }

        protected UmbracoHalContentControllerBase(
            UmbracoContext umbracoContext, 
            UmbracoHelper umbracoHelper)
            : base(umbracoContext, umbracoHelper)
        {
        }

        [HttpGet]
        [CustomRoute("{id}/children/{pageIndex?}/{pageSize?}")]
        public HttpResponseMessage GetChildren(TId id, long pageIndex = 0, int pageSize = 100)
        {
            var result = GetChildContent(id, pageIndex, pageSize);
            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                : Request.CreateResponse(HttpStatusCode.OK, CreatePagedContentRepresentation(
                    result,
                    LinkTemplate.PagedChildren,
                    new { id = id }));
        }

        [HttpGet]
        [CustomRoute("{id}/descendants/{pageIndex?}/{pageSize?}")]
        public HttpResponseMessage GetDescendants(TId id, long pageIndex = 0, int pageSize = 100)
        {
            var result = GetDescendantContent(id, pageIndex, pageSize);
            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                : Request.CreateResponse(HttpStatusCode.OK, CreatePagedContentRepresentation(
                    result,
                    LinkTemplate.PagedDescendants,
                    new { id = id }));
        }

    }
}