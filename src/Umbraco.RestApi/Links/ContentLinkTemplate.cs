using Umbraco.RestApi.Routing;
using WebApi.Hal;

namespace Umbraco.RestApi.Links
{
    public class ContentLinkTemplate : IContentLinkTemplate<int>
    {
        private readonly int _version;

        public ContentLinkTemplate(int version)
        {
            _version = version;
        }

        public Link Root
        {
            get { return new Link("root", string.Format("~/{0}/{1}", RouteConstants.GetRestRootPath(_version), RouteConstants.ContentSegment)); }
        }

        public Link Self
        {
            get { return new Link("content", string.Format("~/{0}/{1}/{{id}}", RouteConstants.GetRestRootPath(_version), RouteConstants.ContentSegment)); }
        }

        public Link Parent
        {
            get { return new Link("parent", string.Format("~/{0}/{1}/{{parentId}}", RouteConstants.GetRestRootPath(_version), RouteConstants.ContentSegment)); }
        }

        public Link PagedChildren(int id)
        {
            return new Link("children", 
                string.Format("~/{0}/{1}/{2}/children{{?pageIndex,pageSize}}", RouteConstants.GetRestRootPath(_version), RouteConstants.ContentSegment, id));
        }

        public Link PagedDescendants(int id)
        {
            return new Link("descendants", 
                string.Format("~/{0}/{1}/{2}/descendants{{?pageIndex,pageSize}}", RouteConstants.GetRestRootPath(_version), RouteConstants.ContentSegment, id));
        }

        public Link MetaData
        {
            get { return new Link("meta", string.Format("~/{0}/{1}/{{id}}/meta", RouteConstants.GetRestRootPath(_version), RouteConstants.ContentSegment)); }
        }

        public Link Search
        {
            get { return new Link("search", string.Format("~/{0}/{1}/search{{?pageIndex,pageSize,lucene}}", RouteConstants.GetRestRootPath(_version), RouteConstants.ContentSegment)); }
        }

        public Link Upload
        {
            get { return new Link("upload", string.Format("~/{0}/{1}/{{id}}/upload{{?property}}", RouteConstants.GetRestRootPath(_version), RouteConstants.ContentSegment)); }
        }
    }
}