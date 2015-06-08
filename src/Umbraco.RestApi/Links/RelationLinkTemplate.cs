using Umbraco.RestApi.Routing;
using WebApi.Hal;

namespace Umbraco.RestApi.Links
{
    public class RelationLinkTemplate : IRelationLinkTemplate
    {
        private readonly int _version;

        public RelationLinkTemplate(int version)
        {
            _version = version;
        }

        public Link RootContent
        {
            get { return new Link("root", string.Format("~/{0}/{1}", RouteConstants.GetRestRootPath(_version), RouteConstants.RelationsSegment)); }
        }

        public Link ContentItem
        {
            get { return new Link("relation", string.Format("~/{0}/{1}/{{id}}", RouteConstants.GetRestRootPath(_version), RouteConstants.RelationsSegment)); }
        }

        public Link Children
        {
            get { return new Link("relatedChildren", string.Format("~/{0}/{1}/children/{{id}}", RouteConstants.GetRestRootPath(_version), RouteConstants.RelationsSegment)); }
        }

        public Link Parents
        {
            get { return new Link("relatedParents", string.Format("~/{0}/{1}/parents/{{id}}", RouteConstants.GetRestRootPath(_version), RouteConstants.RelationsSegment)); }
        }

     }
}