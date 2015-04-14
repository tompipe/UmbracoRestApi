using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing;
using CollectionJson;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Rest.Routing;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Rest.Serialization
{
    /// <summary>
    /// A writer for IContent
    /// </summary>
    public class ContentDocumentWriter : UmbracoDocumentWriterBase, ICollectionJsonDocumentWriter<IContent>
    {
        private readonly IContentService _contentService;

        public ContentDocumentWriter(HttpRequestMessage request, UrlProvider urlProvider, IContentService contentService)
            : base(request, urlProvider)
        {
            _contentService = contentService;
        }

        public IReadDocument Write(IEnumerable<IContent> items)
        {
            //TODO: If we are actually going to be adding Prompt's then they should be localized

            var document = CreateDocument();

            foreach (var content in items)
            {
                //TODO: We should create a single query for all content item id's to check if any of them have children and then use that resolved collection here
                // otherwise we'll end up with N+1 queries because we'd have to make a service call for each item to see if it has children
                var hasChildren = _contentService.GetChildren(content.Id).Any();

                var item = CreateContentItem(
                   content.Id, content.Name, content.Path, content.Level, content.SortOrder, content.ContentType.Alias,
                   hasChildren, content.ParentId,
                   //NOTE: we are passing empty strings for the creator and writer name because if we use looked them up we'd have tons of N+1
                   content.CreatorId, string.Empty, content.WriterId, string.Empty);

                foreach (var property in content.Properties)
                {
                    CreatePropertyData(item, content, property.Alias);
                }    

                document.Collection.Items.Add(item);
            }

            //TODO: Add real query rels here when we create the service

            //var query = new Query { Rel = "search", Href = new Uri(_requestUri, "/api/content/v1/"), Prompt = "Search" };
            //query.Data.Add(new Data { Name = "name", Prompt = "Search Term" });
            //document.Collection.Queries.Add(query);

            //TODO: When we implement create/update the template needs to exist so consumers know what data to send up, when we define that
            // we need to write the template

            //var data = document.Collection.Template.Data;
            //data.Add(new Data { Name = "name", Prompt = "Name" });
            //data.Add(new Data { Name = "path", Prompt = "Path" });
            //data.Add(new Data { Name = "level", Prompt = "Level" });
            //data.Add(new Data { Name = "sortorder", Prompt = "Sort Order" });
            //data.Add(new Data { Name = "contenttypealias", Prompt = "ContentType Alias" });

            return document;
        }

        protected override string BaseRouteName
        {
            get { return RouteConstants.ContentRouteName; }
        }
    }
}