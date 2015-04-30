using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using AutoMapper;
using Umbraco.Web.Rest.Links;
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

        #region Actions
        public HttpResponseMessage Get()
        {
            var result = GetRootContent();
            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                : Request.CreateResponse(HttpStatusCode.OK, CreateContentRepresentation(result));
        }

        public HttpResponseMessage Get(TId id)
        {
            var result = GetItem(id);
            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotFound)
                : Request.CreateResponse(HttpStatusCode.OK, CreateContentRepresentation(result));
        }

        [HttpGet]
        [ActionName("children")]
        public virtual HttpResponseMessage GetChildren(TId id)
        {
            var result = GetChildContent(id);
            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                : Request.CreateResponse(HttpStatusCode.OK, CreateContentRepresentation(result));
        }

        public HttpResponseMessage Post(ContentRepresentation content)
        {
            var result = CreateNew(content);
            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                : content == null
                ? Request.CreateResponse(HttpStatusCode.BadRequest, "content is null")
                : Request.CreateResponse(HttpStatusCode.Created, CreateContentRepresentation(result));
        }

        public HttpResponseMessage Put(TId id, ContentRepresentation content)
        {
            var result = Update(id, content);
            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                : content == null
                ? Request.CreateResponse(HttpStatusCode.BadRequest, "content is null")
                : Request.CreateResponse(HttpStatusCode.OK, CreateContentRepresentation(result));
        }

        public virtual HttpResponseMessage Delete(int id)
        {
            return Request.CreateResponse(HttpStatusCode.NotImplemented);
        } 
        #endregion

        #region Protected - to override for REST implementation

        protected abstract TEntity GetItem(TId id);

        protected virtual IEnumerable<TEntity> GetRootContent()
        {
            return null;
        }

        protected virtual IEnumerable<TEntity> GetChildContent(TId id)
        {
            return null;
        }

        protected virtual TEntity CreateNew(ContentRepresentation content)
        {
            return null;
        }

        protected virtual TEntity Update(TId id, ContentRepresentation content)
        {
            return null;
        } 

        #endregion

        protected abstract IContentLinkTemplate LinkTemplate { get; }

        /// <summary>
        /// Returns the current version request
        /// </summary>
        protected int CurrentVersionRequest
        {
            get { return int.Parse(Regex.Match(Request.RequestUri.AbsolutePath, "/v(\\d+)/", RegexOptions.Compiled).Groups[1].Value); }
        }

        /// <summary>
        /// Creates the content list representation from the entities based on the current API version
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        protected ContentListRepresentation CreateContentRepresentation(IEnumerable<TEntity> entities)
        {
            return new ContentListRepresentation(entities.Select(CreateContentRepresentation).ToList());
        }

        /// <summary>
        /// Creates the content representation from the entity based on the current API version
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected ContentRepresentation CreateContentRepresentation(TEntity entity)
        {
            //create it with the current version link representation
            var representation = new ContentRepresentation(LinkTemplate);
            return Mapper.Map(entity, representation);
        }

        protected HttpResponseException ValidationException(ModelStateDictionary modelState, string message = null, params string[] errors)
        {
            var errorList = (from ms in modelState
                from error in ms.Value.Errors
                select new ValidationError {Field = ms.Key, Message = error.ErrorMessage})
                .ToList();

            //add additional messages
            foreach (var error in errors)
            {
                errorList.Add(new ValidationError {Message = error});
            }

            var errorModel = new ValidationResponse
            {
                HttpStatusCode = (int) HttpStatusCode.BadRequest,
                Message = message ?? "Validation error occurred",
                Errors = errorList,
                Time = DateTime.Now
            };

            return new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest, errorModel));
        }

        ///// <summary>
        ///// Create a 400 response message indicating that a validation error occurred
        ///// 
        ///// </summary>
        ///// <param name="request"/><param name="errorMessage"/>
        ///// <returns/>
        //public static HttpResponseMessage CreateValidationErrorResponse(this HttpRequestMessage request, string errorMessage)
        //{
        //    HttpResponseMessage errorResponse = System.Net.Http.HttpRequestMessageExtensions.CreateErrorResponse(request, HttpStatusCode.BadRequest, errorMessage);
        //    errorResponse.Headers.Add("X-Status-Reason", "Validation failed");
        //    return errorResponse;
        //}

        ///// <summary>
        ///// Create a 400 response message indicating that a validation error occurred
        ///// 
        ///// </summary>
        ///// <param name="request"/><param name="modelState"/>
        ///// <returns/>
        //public static HttpResponseMessage CreateValidationErrorResponse(this HttpRequestMessage request, ModelStateDictionary modelState)
        //{
        //    HttpResponseMessage errorResponse = System.Net.Http.HttpRequestMessageExtensions.CreateErrorResponse(request, HttpStatusCode.BadRequest, modelState);
        //    errorResponse.Headers.Add("X-Status-Reason", "Validation failed");
        //    return errorResponse;
        //}
    }
}