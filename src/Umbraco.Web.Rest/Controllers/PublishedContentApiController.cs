using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
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
    [UmbracoRoutePrefix("v1/content")]
    public class PublishedContentApiController : UmbracoCollectionJsonController
    {
        //TODO: We need to make a way to return IPublishedContent from either the cache or from Examine, then convert that to the output
        // this controller needs to support both data sources in one way or another - either base classes, etc...

        protected ICollectionJsonDocumentWriter<IPublishedContent> Writer { get; set; }

        /// <summary>
        /// Default ctor
        /// </summary>
        public PublishedContentApiController()
        {
            Writer = new ContentDocumentWriter(Request);
        }

        /// <summary>
        /// All dependencies
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="writer"></param>
        /// <param name="umbracoHelper"></param>
        public PublishedContentApiController(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper, ICollectionJsonDocumentWriter<IPublishedContent> writer)
            : base(umbracoContext, umbracoHelper)
        {
            Writer = writer;
        }

        /// <summary>
        /// Get a collection of root nodes
        /// </summary>
        /// <returns></returns>
        [Route("")]
        public override HttpResponseMessage Get()
        {
            var response = new HttpResponseMessage();
            var items = this.Umbraco.TypedContentAtRoot();
            var readDoc = Writer.Write(items);
            response.Content = readDoc.ToObjectContent();
            return response;
        }

        /// <summary>
        /// Returns a single item with traversal rels
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id:int}")]
        public override HttpResponseMessage Get(int id)
        {
            var response = new HttpResponseMessage();
            var publishedContent = this.Umbraco.TypedContent(id);
            var readDoc = Writer.Write(publishedContent);
            response.Content = readDoc.ToObjectContent();
            return response;
        }

        /// <summary>
        /// Returns the children for an item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id:int}/children")]
        public override HttpResponseMessage GetChildren(int id)
        {
            var response = new HttpResponseMessage();
            var publishedContent = this.Umbraco.TypedContent(id);
            var items = publishedContent.Children;
            var readDoc = Writer.Write(items);
            response.Content = readDoc.ToObjectContent();
            return response;
        }

    }
}