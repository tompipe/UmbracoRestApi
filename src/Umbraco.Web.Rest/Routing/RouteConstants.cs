using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace Umbraco.Web.Rest.Routing
{
    public static class RouteConstants
    {
        public const string CollectionJsonPrefix = "cj";
        public const string ODataPrefix = "odata";

        public const string PublishedSegment = "published";
        public const string ContentSegment = "Content";
        public const string MediaSegment = "media";
        public const string MembersSegment = "members";

        public const string PublishedContentRouteName = "UR_PublishedContent";
        public const string ContentRouteName = "UR_Content";
        public const string MediaRouteName = "UR_Media";
        public const string MembersRouteName = "UR_Members";

        /// <summary>
        /// Gets the route name for the GET requests
        /// </summary>
        /// <param name="baseRouteName"></param>
        /// <returns></returns>
        public static string GetRouteNameForGetRequests(string baseRouteName)
        {
            return baseRouteName + "_GET";
        }
    }
}