using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;
using CollectionJson;
using Umbraco.Core.Models;
using Umbraco.Web.Rest.Routing;

namespace Umbraco.Web.Rest.Serialization
{
    /// <summary>
    /// A writer for IPublishedContent
    /// </summary>
    public class PublishedContentDocumentWriter : UmbracoDocumentWriterBase, ICollectionJsonDocumentWriter<IPublishedContent>
    {
        public PublishedContentDocumentWriter(HttpRequestMessage request)
            : base(request)
        {
        }

        public IReadDocument Write(IEnumerable<IPublishedContent> items)
        {
            //TODO: If we are actually going to be adding Prompt's then they should be localized

            var document = CreateDocument();

            foreach (var content in items)
            {
                var item = CreateContentItem(
                    content.Id, content.Name, content.Path, content.Level, content.SortOrder, content.DocumentTypeAlias,
                    content.Children.Any(), content.Parent == null ? -1 : content.Parent.Id);

                document.Collection.Items.Add(item);
            }

            //TODO: Add real query rels here when we create the service

            //var query = new Query { Rel = "search", Href = new Uri(_requestUri, "/api/content/v1/"), Prompt = "Search" };
            //query.Data.Add(new Data { Name = "name", Prompt = "Search Term" });
            //collection.Queries.Add(query);

            //NOTE: we don't supply template data for published content, because you cannot create or update it, it is readonly.
            // updating/creating can be done with the normal content

            return document;
        }

        protected override string BaseRouteName
        {
            get { return RouteConstants.PublishedContentRouteName; }
        }
    }
}
