using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CollectionJson;
using CollectionJson.Server;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Rest.Controllers
{
    /// <summary>
    /// Base CollectionJson controller for all Umbraco REST services
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <remarks>
    /// This is taken from the collection json source but modified with our own base class and attributes
    /// </remarks>
    [UmbracoAuthorize]
    [IsBackOffice]
    [CollectionJsonFormatterConfiguration]
    public abstract class UmbracoCollectionJsonController<TData, TId> : UmbracoApiControllerBase
    {
        private readonly string _routeName;

        protected UmbracoCollectionJsonController()
        {
        }

        protected UmbracoCollectionJsonController(
            UmbracoContext umbracoContext, 
            UmbracoHelper umbracoHelper, 
            ICollectionJsonDocumentWriter<TData> writer, 
            ICollectionJsonDocumentReader<TData> reader,
            //TODO: I believe this might need to be required for this all to work
            string routeName = "DefaultApi")
            : base(umbracoContext, umbracoHelper)
        {
            this._routeName = routeName;
            this.Writer = writer;
            this.Reader = reader;
        }

        protected ICollectionJsonDocumentWriter<TData> Writer { get; set; }
        protected ICollectionJsonDocumentReader<TData> Reader { get; set; }

        private string ControllerName
        {
            get { return this.ControllerContext.ControllerDescriptor.ControllerName; }
        }

        /// <summary>
        /// Get a collection of root nodes
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Get()
        {
            var response = new HttpResponseMessage();
            var readDocument = this.Read(response);
            response.Content = readDocument.ToObjectContent();
            return response;
        }

        /// <summary>
        /// Returns a single item with traversal rels
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Get(TId id)
        {
            var response = new HttpResponseMessage();
            var readDocument = this.Read(id, response);
            response.Content = readDocument.ToObjectContent();
            return response;
        }

        /// <summary>
        /// Returns the children for an item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("children")]
        public HttpResponseMessage GetChildren(TId id)
        {
            var response = new HttpResponseMessage();
            var readDocument = this.ReadChildren(id, response);
            response.Content = readDocument.ToObjectContent();
            return response;
        }

        [HttpPost]
        public HttpResponseMessage Post(IWriteDocument document)
        {
            var response = new HttpResponseMessage(HttpStatusCode.Created);
            var id = Create(document, response);
            response.Headers.Location = new Uri(Url.Link(this._routeName, new { controller = this.ControllerName, id = id }));
            return response;
        }

        [HttpPut]
        public HttpResponseMessage Put(TId id, IWriteDocument document)
        {
            var response = new HttpResponseMessage();
            var readDocument = this.Update(id, document, response);
            response.Content = readDocument.ToObjectContent();
            return response;
        }

        [HttpDelete]
        public HttpResponseMessage Remove(TId id)
        {
            var response = new HttpResponseMessage();
            Delete(id, response);
            return response;
        }

        protected virtual TId Create(IWriteDocument document, HttpResponseMessage response)
        {
            throw new HttpResponseException(System.Net.HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Returns a single item with traversal rels
        /// </summary>
        /// <param name="id"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        protected virtual IReadDocument Read(TId id, HttpResponseMessage response)
        {
            throw new HttpResponseException(System.Net.HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Get a collection of root nodes
        /// </summary>
        /// <returns></returns>
        protected virtual IReadDocument Read(HttpResponseMessage response)
        {
            throw new HttpResponseException(System.Net.HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Returns the children for an item
        /// </summary>
        /// <returns></returns>
        protected virtual IReadDocument ReadChildren(TId id, HttpResponseMessage response)
        {
            throw new HttpResponseException(System.Net.HttpStatusCode.NotImplemented);
        }

        protected virtual IReadDocument Update(TId id, IWriteDocument writeDocument, HttpResponseMessage response)
        {
            throw new HttpResponseException(System.Net.HttpStatusCode.NotImplemented);
        }

        protected virtual void Delete(TId id, HttpResponseMessage response)
        {
            throw new HttpResponseException(System.Net.HttpStatusCode.NotImplemented);
        }
    }


}