using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.RestApi.Routing;
using WebApi.Hal;

namespace Umbraco.RestApi.Links
{
    public class MembersLinkTemplate : ILinkTemplate
    {
        private readonly int _version;

        public MembersLinkTemplate(int version)
        {
            _version = version;
        }

        public Link Root
        {
            get { return new Link("root", string.Format("~/{0}/{1}{{?pageIndex,pageSize,orderBy,direction,memberTypeAlias,filter}}", RouteConstants.GetRestRootPath(_version), RouteConstants.MembersSegment)); }
        }

        public Link Self
        {
            get { return new Link("member", string.Format("~/{0}/{1}/{{id}}", RouteConstants.GetRestRootPath(_version), RouteConstants.MembersSegment)); }
        }

        public Link ParentContent
        {
            get { return null; }
        }

        public Link PagedDescendantContent
        {
            get { return null; }
        }

        public Link MetaData
        {
            get { return new Link("meta", string.Format("~/{0}/{1}/{{id}}/meta", RouteConstants.GetRestRootPath(_version), RouteConstants.MembersSegment)); }
        }

        public Link Search
        {
            get { return new Link("search", string.Format("~/{0}/{1}/search{{?pageIndex,pageSize,lucene}}", RouteConstants.GetRestRootPath(_version), RouteConstants.MembersSegment)); }
        }

        public Link Upload
        {
            get { return new Link("upload", string.Format("~/{0}/{1}/{{id}}/upload{{?property}}", RouteConstants.GetRestRootPath(_version), RouteConstants.MembersSegment)); }
        }

     
       
      
    }
}
