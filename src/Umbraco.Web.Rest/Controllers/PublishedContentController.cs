using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using CollectionJson;
using Umbraco.Core.Models;
using Umbraco.Web.Rest.Routing;
using Umbraco.Web.Rest.Serialization;
using Umbraco.Web.Security;

namespace Umbraco.Web.Rest.Controllers
{
    /// <summary>
    /// REST service for querying against Published content
    /// </summary>    
    public class PublishedContentController : UmbracoCollectionJsonController<IPublishedContent, int>
    {
        //TODO: We need to make a way to return IPublishedContent from either the cache or from Examine, then convert that to the output
        // this controller needs to support both data sources in one way or another - either base classes, etc...

        /// <summary>
        /// Default ctor
        /// </summary>
        public PublishedContentController()
        {
            Writer = new PublishedContentDocumentWriter(Request);
        }

        /// <summary>
        /// All dependencies
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="writer"></param>
        /// <param name="umbracoHelper"></param>
        /// <param name="reader"></param>
        public PublishedContentController(
            UmbracoContext umbracoContext, 
            UmbracoHelper umbracoHelper, 
            ICollectionJsonDocumentWriter<IPublishedContent> writer, 
            ICollectionJsonDocumentReader<IPublishedContent> reader)
            : base(umbracoContext, umbracoHelper, writer, reader)
        {
        }

        /// <summary>
        /// Get a collection of root nodes
        /// </summary>
        /// <returns></returns>
        protected override IReadDocument Read(HttpResponseMessage response)
        {
            var items = Umbraco.TypedContentAtRoot();
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
            var content = Umbraco.TypedContent(id);
            if (content == null) throw new HttpResponseException(HttpStatusCode.NotFound);
            return Writer.Write(content);
        }

        /// <summary>
        /// Returns the children for an item
        /// </summary>
        /// <returns></returns>
        protected override IReadDocument ReadChildren(int id, HttpResponseMessage response)
        {
            var content = Umbraco.TypedContent(id);
            return Writer.Write(content == null ? Enumerable.Empty<IPublishedContent>() : content.Children);
        }

    }
}