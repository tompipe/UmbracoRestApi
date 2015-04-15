using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using CollectionJson;
using CollectionJson.Server;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Rest.Serialization;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Rest.Controllers
{
    public class ContentController : UmbracoCollectionJsonController<IContent, int>
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
        /// <param name="writer"></param>
        /// <param name="umbracoHelper"></param>
        /// <param name="reader"></param>
        public ContentController(
            UmbracoContext umbracoContext, 
            UmbracoHelper umbracoHelper, 
            ICollectionJsonDocumentWriter<IContent> writer, 
            ICollectionJsonDocumentReader<IContent> reader)
            : base(umbracoContext, umbracoHelper, writer, reader)
        {
        }

        /// <summary>
        /// If the Reader/Writer are null because the empty ctor was used, initialize them
        /// </summary>        
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            if (Reader == null)
            {
                Reader = new ContentDocumentReader(ApplicationContext.Services.ContentService, Logger);
            }
            if (Writer == null)
            {
                Writer = new ContentDocumentWriter(Request, Umbraco.UrlProvider, ApplicationContext.Services.ContentService);
            }
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
        protected override int Create(IWriteDocument document, HttpResponseMessage response)
        {
            //read the request into an IContent instance
            var content = Reader.Read(document);

            //That's it! just save it
            ContentService.Save(content);

            return content.Id;
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
        protected override IReadDocument Update(int id, IWriteDocument writeDocument, HttpResponseMessage response)
        {
            //ensure it exists
            var content = ContentService.GetById(id);
            if (content == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            //read the request into an IContent instance
            var parsedContent = Reader.Read(writeDocument);

            //now perform any required updates
            if (parsedContent.Name != content.Name)
            {
                content.Name = parsedContent.Name;
            }
            
            foreach (var property in parsedContent.Properties)
            {
                var foundPropertyType = content.PropertyTypes.FirstOrDefault(x => x.Alias == property.Alias);
                if (foundPropertyType != null)
                {
                    var foundProperty = content.Properties[foundPropertyType.Alias];
                    //need to use "Equals" here because the underlying object is 'object'
                    if (foundProperty != null && foundProperty.Value.Equals(property.Value) == false)
                    {
                        //update the property value if it is different
                        foundProperty.Value = property.Value;
                    }
                }
            }

            //update
            ContentService.Save(content);

            return Writer.Write(content);
        }

        /// <summary>
        /// Deletes an item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="response"></param>
        protected override void Delete(int id, HttpResponseMessage response)
        {
            var content = ContentService.GetById(id);
            if (content == null) throw new HttpResponseException(HttpStatusCode.NotFound);
            ContentService.Delete(content);
        }
    }
}
