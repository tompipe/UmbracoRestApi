using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CollectionJson;
using CollectionJson.Server;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Web.Rest.Controllers
{
    public class ContentController : UmbracoCollectionJsonController<IContent, int>
    {
        public ContentController()
        {            
        }

        public ContentController(
            UmbracoContext umbracoContext, 
            UmbracoHelper umbracoHelper, 
            ICollectionJsonDocumentWriter<IContent> writer, 
            ICollectionJsonDocumentReader<IContent> reader)
            : base(umbracoContext, umbracoHelper, writer, reader)
        {
        }

        protected IContentService ContentService
        {
            get { return ApplicationContext.Services.ContentService; }
        }

        /// <summary>
        /// Get a collection of root nodes
        /// </summary>
        /// <returns></returns>
        protected override IReadDocument Read(HttpResponseMessage response)
        {
            var items = ContentService.GetRootContent();
            return Writer.Write(items);
        }

        /// <summary>
        /// Returns a single item with traversal rels
        /// </summary>
        /// <param name="id"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        protected override IReadDocument Read(int id, HttpResponseMessage response)
        {
            var content = ContentService.GetById(id);
            if (content == null) throw new HttpResponseException(HttpStatusCode.NotFound);
            return Writer.Write(content);
        }

        /// <summary>
        /// Returns the children for an item
        /// </summary>
        /// <returns></returns>
        protected override IReadDocument ReadChildren(int id, HttpResponseMessage response)
        {
            var content = ContentService.GetChildren(id);
            return Writer.Write(content);
        }
    }
}
