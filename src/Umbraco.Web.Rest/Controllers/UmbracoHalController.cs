using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Umbraco.Web.Rest.Models;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Rest.Controllers
{
    [UmbracoAuthorize]
    [IsBackOffice]
    [HalFormatterConfiguration]
    public abstract class UmbracoHalController<TId, TEntity> : UmbracoApiControllerBase
        where TEntity : class
    {
        protected UmbracoHalController()
        {
        }

        protected UmbracoHalController(
            UmbracoContext umbracoContext, 
            UmbracoHelper umbracoHelper)
            : base(umbracoContext, umbracoHelper)
        {
        }

        public HttpResponseMessage Get()
        {
            var result = GetRootContent();
            return result == null 
                ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                : Request.CreateResponse(HttpStatusCode.OK, Mapper.Map<ContentListRepresentation>(result));
        }

        protected virtual IEnumerable<TEntity> GetRootContent()
        {
            return null;
        }

        public HttpResponseMessage Get(TId id)
        {
            var result = GetItem(id);
            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotFound)
                : Request.CreateResponse(HttpStatusCode.OK, Mapper.Map<ContentRepresentation>(result));
        }

        protected abstract TEntity GetItem(TId id);

        [HttpGet]
        [ActionName("children")]
        public virtual HttpResponseMessage GetChildren(TId id)
        {
            var result = GetChildContent(id);
            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                : Request.CreateResponse(HttpStatusCode.OK, Mapper.Map<ContentListRepresentation>(result));
        }

        protected virtual IEnumerable<TEntity> GetChildContent(TId id)
        {
            return null;
        } 

        public HttpResponseMessage Post(ContentRepresentation content)
        {
            if (content == null) return Request.CreateResponse(HttpStatusCode.BadRequest, "content is null");
            var result = CreateNew(content);
            return result == null 
                ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                : Request.CreateResponse(HttpStatusCode.Created, Mapper.Map<ContentRepresentation>(result));
        }

        protected virtual TEntity CreateNew(ContentRepresentation content)
        {
            return null;
        }

        public HttpResponseMessage Put(TId id, ContentRepresentation content)
        {
            if (content == null) return Request.CreateResponse(HttpStatusCode.BadRequest, "content is null");
            var result = Update(id, content);
            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                : Request.CreateResponse(HttpStatusCode.OK, Mapper.Map<ContentRepresentation>(result));
        }

        protected virtual TEntity Update(TId id, ContentRepresentation content)
        {
            return null;
        }

        public virtual HttpResponseMessage Delete(int id)
        {
            return Request.CreateResponse(HttpStatusCode.NotImplemented);
        }
    }
}