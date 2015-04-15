using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing;
using CollectionJson;
using Umbraco.Core.Models;
using Umbraco.Web.Rest.Routing;
using Umbraco.Web.Routing;
using UmbracoExamine.DataServices;
using IContentService = Umbraco.Core.Services.IContentService;

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

            var totalItems = 0;
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

                CreatePropertyDataForContent(item, content);

                document.Collection.Items.Add(item);
                totalItems++;
            }

            //TODO: Add real query rels here when we create the service

            //var query = new Query { Rel = "search", Href = new Uri(_requestUri, "/api/content/v1/"), Prompt = "Search" };
            //query.Data.Add(new Data { Name = FieldNames.Name, Prompt = "Search Term" });
            //document.Collection.Queries.Add(query);

            //If there is only a single item, then render it's template
            if (totalItems == 1)
            {
                WriteTemplate(document, items.Single());
            }

            return document;
        }

        /// <summary>
        /// This writes out a template for a single content item
        /// </summary>
        /// <param name="document"></param>
        /// <param name="content"></param>
        protected void WriteTemplate(IReadDocument document, IContent content)
        {
            //Generate the JSON template for use with updating/persisting
            // ensure not to enclude any readonly values like:
            // * creator or writer, 
            // * contentTypeAlias
            // * sortOrder (which could be writable but would be a bit trickier since we'd need to auto sort the rest)
            // * level, path (both of which would require a move operation)

            var templateData = document.Collection.Template.Data;
            templateData.Add(new Data { Name = FieldNames.Name, Value = content.Name});
            //TODO: What else is directly writable? maybe template, publish at, expire at?
            
            //this is a custom nested template for property values
            var propertiesData = new Data { Name = FieldNames.Properties };
            var propertyList = new List<Data>();

            foreach (var propertyType in content.PropertyTypes)
            {
                var properties = new List<Data>
                {
                    new Data {Name = FieldNames.Alias, Value = propertyType.Alias},
                    new Data
                    {
                        Name = FieldNames.Value,
                        Value = content.Properties[propertyType.Alias].Value == null
                            ? ""
                            : content.Properties[propertyType.Alias].Value.ToString()
                    }
                };
                var prop = new Data();
                prop.SetValue("data", properties);
                propertyList.Add(prop);
            }

            propertiesData.SetValue("items", propertyList);
            templateData.Add(propertiesData);
        }

        protected override string BaseRouteName
        {
            get { return RouteConstants.ContentRouteName; }
        }
    }
}