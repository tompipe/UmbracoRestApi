using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CollectionJson;
using CollectionJson.Server;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Rest.Routing;
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
        protected UmbracoCollectionJsonController()
        {
        }

        protected UmbracoCollectionJsonController(
            UmbracoContext umbracoContext, 
            UmbracoHelper umbracoHelper, 
            ICollectionJsonDocumentWriter<TData> writer, 
            ICollectionJsonDocumentReader<TData> reader)
            : base(umbracoContext, umbracoHelper)
        {
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

            //generate the URL for the GET by ID for this item
            var url = Url.Link(
                RouteConstants.GetRouteNameForGetRequests(ControllerContext.RouteData.Route.GetRouteName()),
                new {controller = this.ControllerName, id = id});

            response.Headers.Location = new Uri(url);
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

        /// <summary>
        /// Creates an item
        /// </summary>
        /// <param name="document"></param>
        /// <param name="response"></param>
        /// <returns>
        /// the server responds with a status code of 201 and a Location header that contains the URI of the newly created item resource.
        /// </returns>
        /// <remarks>
        /// http://amundsen.com/media-types/collection/format/#read-write  see: 2.1.2. Adding an Item
        /// </remarks>
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

        /// <summary>
        /// Updates an item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="writeDocument"></param>
        /// <param name="response"></param>
        /// <returns>
        /// The server will respond with HTTP status code 200 and a representation of the updated item resource representation.
        /// </returns>
        /// <remarks>
        /// http://amundsen.com/media-types/collection/format/#read-write  see: 2.1.4. Updating an Item
        /// </remarks>
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