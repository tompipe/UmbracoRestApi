using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CollectionJson;
using Umbraco.Core.Models;

namespace Umbraco.Web.Rest.Serialization
{
    /// <summary>
    /// A writer for IPublishedContent
    /// </summary>
    public class PublishedContentDocumentWriter : ICollectionJsonDocumentWriter<IPublishedContent>
    {
        private readonly Uri _requestUri;

        public PublishedContentDocumentWriter(HttpRequestMessage request)
        {
            _requestUri = request.RequestUri;
        }

        public IReadDocument Write(IEnumerable<IPublishedContent> items)
        {
            //TODO: If we are actually going to be adding Prompt's then they should be localized

            var document = new ReadDocument();
            var collection = new Collection { Version = "1.0", Href = new Uri(_requestUri, "/api/content/v1/") };
            document.Collection = collection;

            foreach (var content in items)
            {
                var item = new Item { Href = new Uri(_requestUri, "/api/content/v1/" + content.Id) };
                item.Data.Add(new Data { Name = "name", Value = content.Name, Prompt = "Name" });
                item.Data.Add(new Data { Name = "path", Value = content.Path, Prompt = "Path" });
                item.Data.Add(new Data { Name = "level", Value = content.Level.ToString(CultureInfo.InvariantCulture), Prompt = "Level" });
                item.Data.Add(new Data { Name = "sortorder", Value = content.SortOrder.ToString(CultureInfo.InvariantCulture), Prompt = "Sort Order" });
                item.Data.Add(new Data { Name = "contenttypealias", Value = content.DocumentTypeAlias, Prompt = "ContentType Alias" });
                if (content.Children.Any())
                    item.Links.Add(new Link { Rel = "children", Href = new Uri(_requestUri, string.Format("/api/content/v1/{0}/children", content.Id)), Prompt = "Children" });
                if (content.Parent != null)
                    item.Links.Add(new Link { Rel = "parent", Href = new Uri(_requestUri, "/api/content/v1/" + content.Parent.Id), Prompt = "Parent" });
                collection.Items.Add(item);
            }

            var query = new Query { Rel = "search", Href = new Uri(_requestUri, "/api/content/v1/"), Prompt = "Search" };
            query.Data.Add(new Data { Name = "name", Prompt = "Search Term" });
            collection.Queries.Add(query);

            var data = collection.Template.Data;
            data.Add(new Data { Name = "name", Prompt = "Name" });
            data.Add(new Data { Name = "path", Prompt = "Path" });
            data.Add(new Data { Name = "level", Prompt = "Level" });
            data.Add(new Data { Name = "sortorder", Prompt = "Sort Order" });
            data.Add(new Data { Name = "contenttypealias", Prompt = "ContentType Alias" });
            return document;
        }
    }
}
