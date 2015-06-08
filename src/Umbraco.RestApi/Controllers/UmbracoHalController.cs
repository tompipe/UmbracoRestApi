using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using AutoMapper;
using Examine;
using Examine.Providers;
using Newtonsoft.Json.Serialization;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.RestApi.Links;
using Umbraco.RestApi.Models;
using Umbraco.RestApi.Routing;
using Umbraco.Web;
using Umbraco.Web.WebApi;
using WebApi.Hal;
using System.Threading.Tasks;


namespace Umbraco.RestApi.Controllers
{
    [DynamicCors]
    [UmbracoAuthorize]
    [IsBackOffice]    
    [HalFormatterConfiguration]
    public abstract class UmbracoHalController<TId, TEntity, TRepresentation, TLinkTemplate> : UmbracoApiControllerBase
        where TEntity : class
        where TId: struct
        where TLinkTemplate : ILinkTemplate
        where TRepresentation: UmbracoRepresentationBase
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

        //NOTE: We cannot accept POST here for now unless we modify the routing structure since there's only one POST per
        // controller currently (with the way we've routed).
        [HttpGet]
        [CustomRoute("search")]
        public HttpResponseMessage Search(
            [ModelBinder(typeof(QueryStructureModelBinder))]
            QueryStructure query)
        {
            var result = PerformSearch(query);
            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                : Request.CreateResponse(HttpStatusCode.OK, CreatePagedContentRepresentation(
                    result,
                    LinkTemplate.Search,
                    new {lucene = query.Lucene}));

        }

        [HttpGet]
        [CustomRoute("")]
        public HttpResponseMessage Get()
        {
            var result = GetRootContent();
            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                : Request.CreateResponse(HttpStatusCode.OK, CreateContentRepresentation(result));
        }

        [HttpGet]
        [CustomRoute("{id}")]
        public HttpResponseMessage Get(TId id)
        {
            var result = GetItem(id);
            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotFound)
                : Request.CreateResponse(HttpStatusCode.OK, CreateRepresentation(result));
        }

        

        [HttpGet]
        [CustomRoute("{id}/meta")]
        public HttpResponseMessage GetMetadata(TId id)
        {
            var result = GetMetadataForItem(id);
            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                : Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpPost]
        [CustomRoute("")]
        public HttpResponseMessage Post(TRepresentation content)
        {
            try
            {
                var result = CreateNew(content);
                return result == null
                    ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                    : content == null
                    ? Request.CreateResponse(HttpStatusCode.BadRequest, "content is null")
                    : Request.CreateResponse(HttpStatusCode.Created, CreateRepresentation(result));
            }
            catch (ModelValidationException exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Errors);
            }
        }

        [HttpPut]
        [CustomRoute("{id}")]
        public HttpResponseMessage Put(TId id, TRepresentation content)
        {
            try
            {
                var result = Update(id, content);
                return result == null
                    ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                    : content == null
                    ? Request.CreateResponse(HttpStatusCode.BadRequest, "content is null")
                    : Request.CreateResponse(HttpStatusCode.OK, CreateRepresentation(result));
            }
            catch (ModelValidationException exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Errors);
            }
        }

        [HttpDelete]
        [CustomRoute("{id}")]
        public virtual HttpResponseMessage Delete(int id)
        {
            return Request.CreateResponse(HttpStatusCode.NotImplemented);
        }

        [HttpPut]
        [CustomRoute("{id}/upload")]
        public async Task<HttpResponseMessage> UploadFile(TId id, string property = "umbracoFile")
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, "The request doesn't contain valid content!");
            }

            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                if (provider.Contents.Count != 1)
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "This method only works with a single file at a time");

                StreamContent file = (StreamContent)provider.Contents.First();
                var name = file.Headers.ContentDisposition.FileName;
                var contentType = file.Headers.ContentType;
                var dataStream = await file.ReadAsStreamAsync();

                //build an in-memory file for umbraco
                var httpFile = new Umbraco.RestApi.Models.MemoryFile(dataStream, contentType.ToString(), name);
                var entity = SetFileOnProperty(id, property, httpFile);
                return Request.CreateResponse(HttpStatusCode.OK, entity);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
        #endregion

        #region Protected - to override for REST implementation

        protected virtual PagedResult<TEntity> PerformSearch(QueryStructure query)
        {
            return null;
        } 

        protected abstract ContentMetadataRepresentation GetMetadataForItem(TId id);

        protected abstract TEntity GetItem(TId id);

        protected virtual IEnumerable<TEntity> GetRootContent()
        {
            return null;
        }

        protected virtual PagedResult<TEntity> GetChildContent(TId id, long pageIndex = 0, int pageSize = 100)
        {
            return null;
        }

        protected virtual PagedResult<TEntity> GetDescendantContent(TId id, long pageIndex = 0, int pageSize = 100)
        {
            return null;
        }

        protected virtual TEntity CreateNew(TRepresentation content)
        {
            return null;
        }

        protected virtual TEntity Update(TId id, TRepresentation content)
        {
            return null;
        }

        protected virtual TEntity SetFileOnProperty(TId id, string property, System.Web.HttpPostedFileBase file)
        {
            return null;
        } 

        #endregion

        protected abstract TLinkTemplate LinkTemplate { get; }

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
        protected UmbracoListRepresentation<TRepresentation> CreateContentRepresentation(IEnumerable<TEntity> entities)
        {
            return new UmbracoListRepresentation<TRepresentation>(entities.Select(CreateRepresentation).ToList(), LinkTemplate);
        }

        protected virtual PagedRepresentationList<TRepresentation> CreatePagedContentRepresentation(
            PagedResult<TEntity> pagedResult, 
            Link pagedUriTemplate,
            object uriTemplateParams = null)
        {
            return new PagedRepresentationList<TRepresentation>(
                pagedResult.Items.Select(CreateRepresentation).ToList(),
                pagedResult.TotalItems,
                pagedResult.TotalPages,
                pagedResult.PageNumber - 1,
                Convert.ToInt32(pagedResult.PageSize),
                pagedUriTemplate,
                uriTemplateParams);
        }

        /// <summary>
        /// Creates the content representation from the entity based on the current API version
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected abstract TRepresentation CreateRepresentation(TEntity entity);
        

        /// <summary>
        /// Used to throw validation exceptions
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="content"></param>
        /// <param name="message"></param>
        /// <param name="id"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        protected ModelValidationException ValidationException(
            ModelStateDictionary modelState,
            TRepresentation content,
            string message = null, int? id = null, params string[] errors)
        {
            var metaDataProvider = this.Configuration.Services.GetModelMetadataProvider();
            
            var errorList = new List<ValidationErrorRepresentation>();
            foreach (KeyValuePair<string, ModelState> ms in modelState)
            {
                foreach (var error in ms.Value.Errors)
                {
                    ////hack - because webapi doesn't seem to support an easy way to change the model metadata for a class, we have to manually
                    //// go get the 'display' name from the metadata for the property and use that for the logref otherwise we end up with the c#
                    //// property name (i.e. contentTypeAlias vs ContentTypeAlias). I'm sure there's some webapi way to achieve 
                    //// this by customizing the model metadata but it's not as clear as with MVC which has IMetadataAware attribute
                    var logRef = ms.Key;
                    //var parts = ms.Key.Split('.');
                    //var isContentField = parts.Length == 2 && parts[0] == "content";
                    //if (isContentField)
                    //{
                    //    parts[1] = metaDataProvider.GetMetadataForProperty(() => content, typeof (ContentRepresentation), parts[1])
                    //                .GetDisplayName();
                    //    logRef = string.Join(".", parts);
                    //}

                    errorList.Add(new ValidationErrorRepresentation
                    {
                        LogRef = logRef,
                        Message = error.ErrorMessage
                    });
                }
                    
            }
                

            //add additional messages
            foreach (var error in errors)
            {
                errorList.Add(new ValidationErrorRepresentation {Message = error});
            }

            var errorModel = new ValidationErrorListRepresentation(errorList, LinkTemplate, id)
            {
                HttpStatus = (int) HttpStatusCode.BadRequest,
                Message = message ?? "Validation errors occurred"                
            };

            return new ModelValidationException(errorModel);
        }

        /// <summary>
        /// Calculates the skip size based on the paged parameters specified
        /// </summary>
        /// <remarks>
        /// Returns 0 if the page number or page size is zero
        /// </remarks>
        public int GetSkipSize(long pageIndex, int pageSize)
        {
            if (pageIndex >= 0 && pageSize > 0)
            {
                return Convert.ToInt32((pageIndex) * pageSize);
            }
            return 0;
        }

        [NonAction]
        protected IDictionary<string, ContentPropertyInfo> GetDefaultFieldMetaData()
        {
            //TODO: This shouldn't actually localize based on the current user!!!
            // this should localize based on the current request's Accept-Language and Content-Language headers

            return new Dictionary<string, ContentPropertyInfo>
            {
                {"id", new ContentPropertyInfo{Label = "Id", ValidationRequired = true}},
                {"key", new ContentPropertyInfo{Label = "Key", ValidationRequired = true}},
                {"contentTypeAlias", new ContentPropertyInfo{Label = TextService.Localize("content/documentType", UserCulture), ValidationRequired = true}},
                {"parentId", new ContentPropertyInfo{Label = "Parent Id", ValidationRequired = true}},
                {"hasChildren", new ContentPropertyInfo{Label = "Has Children"}},
                {"templateId", new ContentPropertyInfo{Label = TextService.Localize("template/template", UserCulture) + " Id", ValidationRequired = true}},
                {"sortOrder", new ContentPropertyInfo{Label = TextService.Localize("general/sort", UserCulture)}},
                {"name", new ContentPropertyInfo{Label = TextService.Localize("general/name", UserCulture), ValidationRequired = true}},
                {"urlName", new ContentPropertyInfo{Label = TextService.Localize("general/url", UserCulture) + " " + TextService.Localize("general/name", UserCulture)}},
                {"writerName", new ContentPropertyInfo{Label = TextService.Localize("content/updatedBy", UserCulture)}},
                {"creatorName", new ContentPropertyInfo{Label = TextService.Localize("content/createBy", UserCulture)}},
                {"writerId", new ContentPropertyInfo{Label = "Writer Id"}},
                {"creatorId", new ContentPropertyInfo{Label = "Creator Id"}},
                {"path", new ContentPropertyInfo{Label = TextService.Localize("general/path", UserCulture)}},
                {"createDate", new ContentPropertyInfo{Label = TextService.Localize("content/createDate", UserCulture)}},
                {"updateDate", new ContentPropertyInfo{Label = TextService.Localize("content/updateDate", UserCulture)}},
                {"level", new ContentPropertyInfo{Label = "Level"}},
                {"url", new ContentPropertyInfo{Label = TextService.Localize("general/url", UserCulture)}},
                {"ItemType", new ContentPropertyInfo{Label = TextService.Localize("general/type", UserCulture)}}
            };
        }

        private CultureInfo _userCulture;
        protected CultureInfo UserCulture
        {
            get { return _userCulture ?? (_userCulture = Security.CurrentUser.GetUserCulture(TextService)); }
        }

        private ILocalizedTextService TextService
        {
            get { return Services.TextService; }
        }
    }
}
