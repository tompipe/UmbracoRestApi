using Umbraco.Web.Rest.Routing;
using WebApi.Hal;

namespace Umbraco.Web.Rest.Links
{
    public class PublishedContentLinkTemplate : IContentLinkTemplate
    {
        private readonly int _version;

        public PublishedContentLinkTemplate(int version)
        {
            _version = version;
        }

        public Link RootContent
        {
            get { return new Link("root", string.Format("~/{0}/{1}/{2}", RouteConstants.GetRestRootPath(_version), RouteConstants.ContentSegment, RouteConstants.PublishedSegment)); }
        }

        public Link ContentItem
        {
            get { return new Link("content", string.Format("~/{0}/{1}/{2}/{{id}}", RouteConstants.GetRestRootPath(_version), RouteConstants.ContentSegment, RouteConstants.PublishedSegment)); }
        }

        public Link ParentContent
        {
            get { return new Link("parent", string.Format("~/{0}/{1}/{2}/{{parentId}}", RouteConstants.GetRestRootPath(_version), RouteConstants.ContentSegment, RouteConstants.PublishedSegment)); }
        }

        public Link ChildContent
        {
            get { return new Link("children", string.Format("~/{0}/{1}/{2}/{{id}}/children", RouteConstants.GetRestRootPath(_version), RouteConstants.ContentSegment, RouteConstants.PublishedSegment)); }
        }
    }
}