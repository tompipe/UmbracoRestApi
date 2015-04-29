using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Rest.Models;

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
            //TODO: Perform validation!!!
            // look into this format: http://soabits.blogspot.dk/2013/05/error-handling-considerations-and-best.html

            var created = ContentService.CreateContent(content.Name, content.ParentId, content.ContentTypeAlias, Security.CurrentUser.Id);

            Mapper.Map(content, created);
            
            ContentService.Save(created);

            return created;
        }

        protected override IContent Update(int id, ContentRepresentation content)
        {
            var found = ContentService.GetById(id);
            if (found == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            Mapper.Map(content, found);

            ContentService.Save(found);

            return found;
        }
       

        protected IContentService ContentService
        {
            get { return ApplicationContext.Services.ContentService; }
        }
    }
}
