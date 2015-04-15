using System.Web.Http.Routing;
using System.Web.Routing;

namespace Umbraco.Web.Rest
{
    internal static class RouteExtensions
    {
        public static void WithRouteName(this Route route, string routeName)
        {
            if (route.DataTokens == null)
            {
                route.DataTokens = new RouteValueDictionary();
            }
            route.DataTokens["UR_RouteName"] = routeName;
        }

        public static void WithRouteName(this IHttpRoute route, string routeName)
        {
            route.DataTokens["UR_RouteName"] = routeName;
        }

        public static string GetRouteName(this IHttpRoute route)
        {
            return route.DataTokens["UR_RouteName"].ToString();
        }
    }
}