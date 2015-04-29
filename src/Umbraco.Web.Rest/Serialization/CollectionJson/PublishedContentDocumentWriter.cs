using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CollectionJson;
using Umbraco.Core.Models;
using Umbraco.Web.Rest.Routing;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Rest.Serialization.CollectionJson
{
    /// <summary>
    /// A writer for IPublishedContent
    /// </summary>
    public class PublishedContentDocumentWriter : UmbracoDocumentWriterBase, ICollectionJsonDocumentWriter<IPublishedContent>
    {
        public PublishedContentDocumentWriter(HttpRequestMessage request, UrlProvider urlProvider)
            : base(request, urlProvider)
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
                    content.Children.Any(), content.Parent == null ? -1 : content.Parent.Id,
                    content.CreatorId, content.CreatorName, content.WriterId, content.WriterName);

                //NOTE: we're going to assume these are in the correct sort order due to how publish cache stores it's values
                foreach (var property in content.Properties)
                {
                    //TODO: Since we are returning this for consumption and since this is published content, I'm thinking that we'd need to put this
                    // value through the property value converters!
                    CreatePropertyData(item, property.PropertyTypeAlias, property.PropertyTypeAlias, property.Value == null ? string.Empty : property.Value.ToString());
                }

                document.Collection.Items.Add(item);
            }

            //TODO: Add real query rels here when we create the service

            //var query = new Query { Rel = "search", Href = new Uri(_requestUri, "/api/content/v1/"), Prompt = "Search" };
            //query.Data.Add(new Data { Name = FieldNames.Name, Prompt = "Search Term" });
            //collection.Queries.Add(query);

            //NOTE: we don't supply template data for published content, because you cannot create or update it, it is readonly.
            // updating/creating can be done with the normal content

            return document;
        }

        protected override string BaseRouteName
        {
            get { return RouteConstants.PublishedContentRouteName + RouteConstants.CollectionJsonPrefix; }
        }
    }
}
