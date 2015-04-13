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
    //TODO: This is from Morten's POC, we can't use this the way it is, here's what needs to happen:
    // *    We need to return a consistent JSON structure for content, media and members, no matter what source
    //      it comes from (services, published cache, examine)
    // *    To do that, we'll either have to: Use our current ContentItemDisplay model and convert the data structures to that model
    //      (but we need to determine if that model is ok for this REST service or not) OR
    //          We create a new, more generic model that we can convert all models to   OR
    //          We create an adapter for each source and do the serialization manually like we do below but ensure it's consistent output
    //              IMO, this is a bad way to do it because it is error prone, we should just create a generic model that we can map
    //              other models too and it's strongly typed!
    public class ContentDocumentWriter : ICollectionJsonDocumentWriter<IPublishedContent>
    {
        private readonly Uri _requestUri;

        public ContentDocumentWriter(HttpRequestMessage request)
        {
            _requestUri = request.RequestUri;
        }

        public IReadDocument Write(IEnumerable<IPublishedContent> items)
        {
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
