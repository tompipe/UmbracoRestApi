using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.RestApi.Links;
using Umbraco.RestApi.Models;
using Umbraco.Web;

namespace Umbraco.RestApi.Controllers
{
    
    public class ContentController : UmbracoHalController<int, IContent>
    {

        /// <summary>
        /// Default ctor
        /// </summary>
        public ContentController()
        {
        }

        /// <summary>
        /// All dependencies
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="umbracoHelper"></param>
        public ContentController(
            UmbracoContext umbracoContext,
            UmbracoHelper umbracoHelper)
            : base(umbracoContext, umbracoHelper)
        {
        }

        protected override IEnumerable<IContent> GetRootContent()
        {
            return ContentService.GetRootContent();
        }

        protected override ContentMetadataRepresentation GetMetadataForItem(int id)
        {
            var found = ContentService.GetById(id);     
            if (found == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            var result = new ContentMetadataRepresentation(LinkTemplate, id)
            {
                Fields = GetDefaultFieldMetaData(),
                Properties = Mapper.Map<IDictionary<string, ContentPropertyInfo>>(found),
                CreateTemplate = Mapper.Map<ContentTemplate>(found)
            };
            return result;
        }

        protected override IContent GetItem(int id)
        {
            return ContentService.GetById(id);                       
        }

        protected override PagedResult<IContent> GetChildContent(int id, long pageIndex = 0, int pageSize = 100)
        {
            long total;
            var items = ContentService.GetPagedChildren(id, pageIndex, pageSize, out total);
            return new PagedResult<IContent>(total, pageIndex + 1, pageSize)
            {
                Items = items
            };
        }

        protected override PagedResult<IContent> GetDescendantContent(int id, long pageIndex = 0, int pageSize = 100)
        {
            long total;
            var items = ContentService.GetPagedDescendants(id, pageIndex, pageSize, out total);
            return new PagedResult<IContent>(total, pageIndex + 1, pageSize)
            {
                Items = items
            };
        }

        protected override IContent CreateNew(ContentRepresentation content)
        {
            //we cannot continue here if the mandatory items are empty (i.e. name, etc...)
            if (!ModelState.IsValid)
            {
                throw ValidationException(ModelState, content);
            }

            var contentType = Services.ContentTypeService.GetContentType(content.ContentTypeAlias);
            if (contentType == null)
            {
                ModelState.AddModelError("content.contentTypeAlias", "No content type found with alias " + content.ContentTypeAlias);
                throw ValidationException(ModelState, content);
            }

            //create an item before persisting of the correct content type
            var created = ContentService.CreateContent(content.Name, content.ParentId, content.ContentTypeAlias, Security.CurrentUser.Id);

            //Validate properties
            var validator = new ContentPropertyValidator<IContent>(ModelState, Services.DataTypeService);
            validator.ValidateItem(content, created);

            if (!ModelState.IsValid)
            {
                throw ValidationException(ModelState, content);
            }

            Mapper.Map(content, created);
            
            ContentService.Save(created);

            return created;
        }

        protected override IContent Update(int id, ContentRepresentation content)
        {
            var found = ContentService.GetById(id);
            if (found == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            //Validate properties
            var validator = new ContentPropertyValidator<IContent>(ModelState, Services.DataTypeService);
            validator.ValidateItem(content, found);

            if (!ModelState.IsValid)
            {
                throw ValidationException(ModelState, content, id: id);
            }

            Mapper.Map(content, found);

            ContentService.Save(found);

            return found;
        }

        protected override IContentLinkTemplate LinkTemplate
        {
            get { return new ContentLinkTemplate(CurrentVersionRequest); }
        }

        protected IContentService ContentService
        {
            get { return ApplicationContext.Services.ContentService; }
        }
    }
   
}
