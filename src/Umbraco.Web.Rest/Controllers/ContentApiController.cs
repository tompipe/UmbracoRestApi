using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using CollectionJson;
using CollectionJson.Client;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Rest.Controllers
{
    [RoutePrefix("api/content/v1")]
    public class ContentApiController : UmbracoApiController
    {
        private static bool _formatterAdded;
        protected ICollectionJsonDocumentWriter<IPublishedContent> Writer { get; set; }

        public ContentApiController(UmbracoContext umbracoContext, ICollectionJsonDocumentWriter<IPublishedContent> writer)
            : base(umbracoContext)
        {
            Writer = writer;
        }

        [Route("")]
        public HttpResponseMessage Get()
        {
            var response = new HttpResponseMessage();
            var items = this.Umbraco.TypedContentAtRoot();
            var readDoc = Writer.Write(items);
            response.Content = readDoc.ToObjectContent();
            return response;
        }

        [Route("{id:int}")]
        public HttpResponseMessage Get(int id)
        {
            var response = new HttpResponseMessage();
            var publishedContent = this.Umbraco.TypedContent(id);
            var readDoc = Writer.Write(publishedContent);
            response.Content = readDoc.ToObjectContent();
            return response;
        }

        [Route("{name}")]
        public HttpResponseMessage Get(string name)
        {
            var items = this.Umbraco.TypedSearch(name);
            var readDocument = Writer.Write(items);
            return readDocument.ToHttpResponseMessage();
        }

        [Route("{id:int}/children")]
        public HttpResponseMessage GetChildren(int id)
        {
            var response = new HttpResponseMessage();
            var publishedContent = this.Umbraco.TypedContent(id);
            var items = publishedContent.Children;
            var readDoc = Writer.Write(items);
            response.Content = readDoc.ToObjectContent();
            return response;
        }

        //protected override void Initialize(HttpControllerContext controllerContext)
        //{
        //    if (!_formatterAdded)
        //    {
        //        lock (_lock)
        //        {
        //            controllerContext.Configuration.Formatters.Insert(0, new CollectionJsonFormatter());
        //            _formatterAdded = true;
        //        }
        //    }
        //    base.Initialize(controllerContext);
        //}
    }
}