using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using CollectionJson;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Rest.Routing;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Rest.Serialization.CollectionJson
{
    public abstract class UmbracoDocumentWriterBase
    {
        protected UmbracoDocumentWriterBase(HttpRequestMessage request, UrlProvider urlProvider)
        {
            UrlProvider = urlProvider;
            RequestUri = request.RequestUri;
            UrlHelper = new UrlHelper(request);
        }

        protected UmbracoDocumentWriterBase(HttpRequestMessage request, UrlProvider urlProvider, UrlHelper urlHelper)
        {
            UrlProvider = urlProvider;
            RequestUri = request.RequestUri;
            UrlHelper = urlHelper;
        }

        protected Uri RequestUri { get; private set; }
        protected UrlHelper UrlHelper { get; private set; }
        protected UrlProvider UrlProvider { get; private set; }

        protected abstract string BaseRouteName { get; }

        /// <summary>
        /// Helper to create the document with the root link
        /// </summary>
        /// <returns></returns>
        public ReadDocument CreateDocument()
        {
            var document = new ReadDocument();
            var collection = new Collection { Version = "1.0", Href = new Uri(RequestUri, GetRootLink()) };
            document.Collection = collection;
            return document;
        }

        /// <summary>
        /// Helper to create a content item
        /// </summary>
        /// <returns></returns>
        public Item CreateContentItem(int contentId, string name, string path, int level, int sortOrder, string contentTypeAlias,
            bool hasChildren, int parentId, int creatorId, string creatorName, int writerId, string writerName)
        {
            //TODO: Need to add validation meta data for each item - whether it's required, data type, regex required, etc... for the client to deal with it

            var item = new Item { Href = new Uri(RequestUri, GetItemLink(contentId)) };
            item.Data.Add(new Data { Name = FieldNames.Name, Value = name, Prompt = "Name" });
            item.Data.Add(new Data { Name = FieldNames.Path, Value = path, Prompt = "Path" });
            item.Data.Add(new Data { Name = FieldNames.Level, Value = level.ToString(CultureInfo.InvariantCulture), Prompt = "Level" });
            item.Data.Add(new Data { Name = FieldNames.SortOrder, Value = sortOrder.ToString(CultureInfo.InvariantCulture), Prompt = "Sort Order" });
            item.Data.Add(new Data { Name = FieldNames.ContentTypeAlias, Value = contentTypeAlias, Prompt = "ContentType Alias" });
            item.Data.Add(new Data { Name = FieldNames.CreatorId, Value = creatorId.ToString(CultureInfo.InvariantCulture), Prompt = "Creator Id" });
            item.Data.Add(new Data { Name = FieldNames.CreatorName, Value = creatorName, Prompt = "Creator Name" });
            item.Data.Add(new Data { Name = FieldNames.WriterId, Value = writerId.ToString(CultureInfo.InvariantCulture), Prompt = "Writer Id" });
            item.Data.Add(new Data { Name = FieldNames.WriterName, Value = writerName, Prompt = "Writer Name" });
            item.Data.Add(new Data { Name = FieldNames.ParentId, Value = parentId.ToInvariantString(), Prompt = "Parent Id" });
            //NOTE: This is just a normal data property, some would argue that it should be a 'link' but it is not a link to more REST
            // resources, it's the actual CMS URl of the item
            item.Data.Add(new Data { Name = FieldNames.Url, Value = GetContentUrl(contentId), Prompt = "Url" });
            
            if (hasChildren)
            {
                item.Links.Add(new Link { Rel = "children", Href = new Uri(RequestUri, GetChildrenLink(contentId)), Prompt = "Children" });
            }

            if (parentId > 0)
            {
                item.Links.Add(new Link { Rel = "parent", Href = new Uri(RequestUri, GetItemLink(parentId)), Prompt = "Parent" });
            }

            return item;
        }

        /// <summary>
        /// Helper to create a property data
        /// </summary>
        /// <param name="item"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyAlias"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Data CreatePropertyData(Item item, string propertyName, string propertyAlias, string value)
        {
            //TODO: Localize prompts
            //TODO: Need to add validation meta data for each item - whether it's required, regex required, etc... for the client to deal with it

            //get the property data item, if it's not there yet, then add it
            var propertyData = item.Data.FirstOrDefault(x => x.Name == FieldNames.Properties);
            if (propertyData == null)
            {
                propertyData = new Data() { Name = FieldNames.Properties, Prompt = "Properties" };                
                item.Data.Add(propertyData);
            }

            //now check for the list value, if its not there create a new one
            //NOTE: on the 'array' syntax: https://github.com/collection-json/extensions/blob/master/value-types.md
            // we're going to use that standard since at least someone else is using it
            var propertyList = propertyData.GetValue<List<Data>>("array") ?? new List<Data>();
            
            //create the name/value pairs for the property data
            var propertyDataList = new List<Data>(new[]
            {
                new Data {Name = FieldNames.Alias, Value = propertyAlias, Prompt = "Alias"},
                new Data {Name = FieldNames.Name, Value = propertyName, Prompt = "Name"},
                new Data {Name = FieldNames.Value, Value = value, Prompt = "Value"}
            });

            //create the new property item data and add it to the list
            var data = new Data();
            
            data.SetValue("data", propertyDataList);            
            propertyList.Add(data);

            //set the custom list back to our custom property data item
            propertyData.SetValue("array", propertyList);

            return data;
        }

        /// <summary>
        /// Helper to create a property data
        /// </summary>
        /// <param name="item"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <remarks>
        /// Since this is for a persistable content property, we also add validation attributes (regexp and required)
        /// </remarks>
        public void CreatePropertyDataForContent(Item item, IContentBase content)
        {
            //TODO: Localize prompts

            foreach (var propertyType in content.PropertyTypes.OrderBy(x => x.SortOrder))
            {
                var propertyData = CreatePropertyData(item,
                    propertyType.Name,
                    propertyType.Alias,
                    //TODO: Since we are returning this for consumption, do we need to put this through the PropertyValueEditor.ConvertDbToEditor ?
                    // I'd assume that we'll have to do that.
                    content.Properties[propertyType.Alias].Value == null ? string.Empty : content.Properties[propertyType.Alias].Value.ToString());

                //TODO: We could look at excluding this data so that it is a smaller payload if the client requests the cut-down version
                // potentially that would be the default and they would have to request the extended version to include all of this stuff
                // if they were doing persistence with a form.

                var dataList = propertyData.GetValue<List<Data>>("data");

                dataList.Add(new Data() { Name = FieldNames.Regexp, Value = propertyType.ValidationRegExp, Prompt = "Validation Pattern" });
                dataList.Add(new Data() { Name = FieldNames.Required, Value = propertyType.Mandatory.ToString().ToLowerInvariant(), Prompt = "Is Required" });
                
                //TODO: Generally with hypermedia we'd also provide the validation message if it failed
            }
        }

        public virtual string GetRootLink()
        {
            var rootLink = UrlHelper.Link(RouteConstants.GetRouteNameForGetRequests(BaseRouteName), new { id = RouteParameter.Optional });
            return rootLink;
        }

        public virtual string GetChildrenLink(int id)
        {
            var rootLink = UrlHelper.Link(RouteConstants.GetRouteNameForGetRequests(BaseRouteName), new { id = id, action = "children" });
            return rootLink;
        }

        public virtual string GetItemLink(int parentId)
        {
            var rootLink = UrlHelper.Link(RouteConstants.GetRouteNameForGetRequests(BaseRouteName), new { id = parentId });
            return rootLink;
        }

        public virtual string GetContentUrl(int id)
        {
            return UrlProvider.GetUrl(id);
        }
    }
}