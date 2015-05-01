using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.RestApi.Links;
using Umbraco.RestApi.Models;
using Umbraco.Web;
using Umbraco.Web.WebApi;

namespace Umbraco.RestApi.Controllers
{
    //TODO: Setup CORS policies, but we need to do this dynamically. We should share how this is done dynamically with the 
    // CORS policies for the auth token provider in the core. 
    // Here's an article on applying policies dynamically: http://blogs.msdn.com/b/webdev/archive/2013/07/02/manage-cors-policy-dynamically.aspx
    // we will do this based on startup code though with some options and can probably use the OwinContext and a per request lifetime options
    // object so we can easily access it in the controllers.

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

        [HttpGet]
        [ActionName("meta")]
        public virtual HttpResponseMessage GetMetadata(TId id)
        {
            var result = GetMetadataForItem(id);
            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                : Request.CreateResponse(HttpStatusCode.OK, result);
        }

        public HttpResponseMessage Post(ContentRepresentation content)
        {
            try
            {
                var result = CreateNew(content);
                return result == null
                    ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                    : content == null
                    ? Request.CreateResponse(HttpStatusCode.BadRequest, "content is null")
                    : Request.CreateResponse(HttpStatusCode.Created, CreateContentRepresentation(result));
            }
            catch (ModelValidationException exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Errors);
            }
        }

        public HttpResponseMessage Put(TId id, ContentRepresentation content)
        {
            try
            {
                var result = Update(id, content);
                return result == null
                    ? Request.CreateResponse(HttpStatusCode.NotImplemented)
                    : content == null
                    ? Request.CreateResponse(HttpStatusCode.BadRequest, "content is null")
                    : Request.CreateResponse(HttpStatusCode.OK, CreateContentRepresentation(result));
            }
            catch (ModelValidationException exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Errors);
            }
        }

        public virtual HttpResponseMessage Delete(int id)
        {
            return Request.CreateResponse(HttpStatusCode.NotImplemented);
        } 
        #endregion

        #region Protected - to override for REST implementation

        protected abstract ContentMetadataRepresentation GetMetadataForItem(TId id);

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
            return new ContentListRepresentation(entities.Select(CreateContentRepresentation).ToList(), LinkTemplate);
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
            ContentRepresentation content,
            string message = null, int? id = null, params string[] errors)
        {
            var metaDataProvider = this.Configuration.Services.GetModelMetadataProvider();
            
            var errorList = new List<ValidationErrorRepresentation>();
            foreach (KeyValuePair<string, ModelState> ms in modelState)
            {
                foreach (var error in ms.Value.Errors)
                {
                    //hack - because webapi doesn't seem to support an easy way to change the model metadata for a class, we have to manually
                    // go get the 'display' name from the metadata for the property and use that for the logref otherwise we end up with the c#
                    // property name (i.e. contentTypeAlias vs ContentTypeAlias). I'm sure there's some webapi way to achieve 
                    // this by customizing the model metadata but it's not as clear as with MVC which has IMetadataAware attribute
                    var logRef = ms.Key;
                    var parts = ms.Key.Split('.');
                    var isContentField = parts.Length == 2 && parts[0] == "content";
                    if (isContentField)
                    {
                        parts[1] = metaDataProvider.GetMetadataForProperty(() => content, typeof (ContentRepresentation), parts[1])
                                    .GetDisplayName();
                        logRef = string.Join(".", parts);
                    }

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


        public IDictionary<string, ContentPropertyInfo> GetDefaultFieldMetaData()
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
