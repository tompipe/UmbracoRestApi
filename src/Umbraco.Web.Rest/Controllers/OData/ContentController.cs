using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using AutoMapper;
using CollectionJson;
using CollectionJson.Client;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Rest.Models;

namespace Umbraco.Web.Rest.Controllers.OData
{
    public class ContentController : UmbracoODataController
    {
        public ContentController()
        {
        }

        public ContentController(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper)
            : base(umbracoContext, umbracoHelper)
        {
        }

        /// <summary>
        /// Returns the root content
        /// </summary>
        /// <returns></returns>
        [EnableQuery]
        public IQueryable<ContentItem> Get()
        {
            var items = ContentService.GetRootContent();
            return Mapper.Map<IEnumerable<ContentItem>>(items).AsQueryable();
        }

        [EnableQuery]
        public ContentItem Get([FromODataUri] int key)
        {
            var content = ContentService.GetById(key);
            if (content == null) throw new HttpResponseException(HttpStatusCode.NotFound);
            return Mapper.Map<ContentItem>(content);
        }

        public IHttpActionResult Post(ContentItem contentItem)
        {
            //TODO: Need to perform all of the validation

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var content = ContentService.CreateContent(contentItem.Name, contentItem.ParentId, contentItem.ContentTypeAlias, Security.CurrentUser.Id);

            Mapper.Map(contentItem, content);

            ContentService.Save(content);

            return Created(Mapper.Map<ContentItem>(content));
        }

        protected IContentService ContentService
        {
            get { return UmbracoContext.Application.Services.ContentService; }
        }


    }
}
