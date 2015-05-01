using Umbraco.RestApi.Routing;
using WebApi.Hal;

namespace Umbraco.RestApi.Links
{
    public class ContentLinkTemplate : IContentLinkTemplate
    {
        private readonly int _version;

        public ContentLinkTemplate(int version)
        {
            _version = version;
        }

        public Link RootContent
        {
            get { return new Link("root", string.Format("~/{0}/{1}", RouteConstants.GetRestRootPath(_version), RouteConstants.ContentSegment)); }
        }

        public Link ContentItem
        {
            get { return new Link("content", string.Format("~/{0}/{1}/{{id}}", RouteConstants.GetRestRootPath(_version), RouteConstants.ContentSegment)); }
        }

        public Link ParentContent
        {
            get { return new Link("parent", string.Format("~/{0}/{1}/{{parentId}}", RouteConstants.GetRestRootPath(_version), RouteConstants.ContentSegment)); }
        }

        public Link ChildContent
        {
            get { return new Link("children", string.Format("~/{0}/{1}/{{id}}/children", RouteConstants.GetRestRootPath(_version), RouteConstants.ContentSegment)); }
        }

        public Link ContentMetaData
        {
            get { return new Link("meta", string.Format("~/{0}/{1}/{{id}}/meta", RouteConstants.GetRestRootPath(_version), RouteConstants.ContentSegment)); }
        }
    }
}