using System;
using System.Globalization;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using CollectionJson;
using Umbraco.Core.Models;
using Umbraco.Web.Rest.Routing;

namespace Umbraco.Web.Rest.Serialization
{
    public abstract class UmbracoDocumentWriterBase
    {
        protected UmbracoDocumentWriterBase(HttpRequestMessage request)
        {
            RequestUri = request.RequestUri;
            UrlHelper = new UrlHelper(request);
        }

        protected Uri RequestUri {get; private set; }
        protected UrlHelper UrlHelper { get; private set; }

        protected abstract string BaseRouteName { get; }

        /// <summary>
        /// Helper to create the document with the root link
        /// </summary>
        /// <returns></returns>
        protected ReadDocument CreateDocument()
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
        protected Item CreateContentItem(object contentId, string name, string path, int level, int sortOrder, string contentTypeAlias,
            bool hasChildren, int parentId)
        {
            var item = new Item { Href = new Uri(RequestUri, GetItemLink(contentId)) };
            item.Data.Add(new Data { Name = "name", Value = name, Prompt = "Name" });
            item.Data.Add(new Data { Name = "path", Value = path, Prompt = "Path" });
            item.Data.Add(new Data { Name = "level", Value = level.ToString(CultureInfo.InvariantCulture), Prompt = "Level" });
            item.Data.Add(new Data { Name = "sortorder", Value = sortOrder.ToString(CultureInfo.InvariantCulture), Prompt = "Sort Order" });
            item.Data.Add(new Data { Name = "contenttypealias", Value = contentTypeAlias, Prompt = "ContentType Alias" });

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
        /// <param name="value"></param>
        /// <returns></returns>
        protected Data CreatePropertyData(Item item, string propertyName, string value)
        {
            //Since this is a user defined property, we need to suffix it because it might overlap with normal content properties
            var serializedPropertyName = "property_" + propertyName;
            var data = new Data {Name = serializedPropertyName, Value = value};
            item.Data.Add(data);
            return data;
        }

        /// <summary>
        /// Helper to create a property data
        /// </summary>
        /// <param name="item"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        protected Data CreatePropertyData(Item item, Property property)
        {
            //TODO: Since we are returning this for consumption, do we need to put this through the PropertyValueEditor.ConvertDbToEditor ?
            // I'd assume that we'll have to do that.
            return CreatePropertyData(item, property.Alias, property.Value == null ? string.Empty : property.Value.ToString());
        }

        protected string GetRootLink()
        {
            var rootLink = UrlHelper.Link(RouteConstants.GetRouteNameForGetRequests(BaseRouteName), new { id = RouteParameter.Optional });
            return rootLink;
        }

        protected string GetChildrenLink(object id)
        {
            var rootLink = UrlHelper.Link(RouteConstants.GetRouteNameForGetRequests(BaseRouteName), new { id = id, action = "children" });
            return rootLink;
        }

        protected string GetItemLink(object parentId)
        {
            var rootLink = UrlHelper.Link(RouteConstants.GetRouteNameForGetRequests(BaseRouteName), new { id = parentId });
            return rootLink;
        }
    }
}