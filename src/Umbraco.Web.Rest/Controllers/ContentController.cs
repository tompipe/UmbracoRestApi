using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Rest.Links;
using Umbraco.Web.Rest.Models;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Rest.Controllers
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
                CreateTemplate = Mapper.Map<ContentTemplate>(found)
            };
            return result;
        }

        protected override IContent GetItem(int id)
        {
            return ContentService.GetById(id);                       
        }

        protected override IEnumerable<IContent> GetChildContent(int id)
        {
            return ContentService.GetChildren(id);
        }

        protected override IContent CreateNew(ContentRepresentation content)
        {
            if (!ModelState.IsValid)
            {
                throw ValidationException(ModelState);
            }

            //TODO: Perform property validation!!!

            var created = ContentService.CreateContent(content.Name, content.ParentId, content.ContentTypeAlias, Security.CurrentUser.Id);

            Mapper.Map(content, created);
            
            ContentService.Save(created);

            return created;
        }

        protected override IContent Update(int id, ContentRepresentation content)
        {
            if (!ModelState.IsValid)
            {
                throw ValidationException(ModelState, id: id);
            }

            //TODO: Perform property validation!!!

            var found = ContentService.GetById(id);
            if (found == null) throw new HttpResponseException(HttpStatusCode.NotFound);

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
