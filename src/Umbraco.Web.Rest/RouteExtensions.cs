using System.Web.Http.Routing;
using System.Web.Routing;

namespace Umbraco.Web.Rest
{
    internal static class RouteExtensions
    {
        public static Route WithRouteName(this Route route, string routeName)
        {
            if (route.DataTokens == null)
            {
                route.DataTokens = new RouteValueDictionary();
            }
            route.DataTokens["UR_RouteName"] = routeName;
            return route;
        }

        public static IHttpRoute WithRouteName(this IHttpRoute route, string routeName)
        {
            route.DataTokens["UR_RouteName"] = routeName;
            return route;
        }

        public static Route WithNamespace(this Route route, string @namespace)
        {
            if (route.DataTokens == null)
            {
                route.DataTokens = new RouteValueDictionary();
            }
            route.DataTokens["Namespaces"] = new string[] {@namespace};
            ;
            return route;
        }

        public static IHttpRoute WithNamespace(this IHttpRoute route, string @namespace)
        {
            route.DataTokens["Namespaces"] = new string[] {@namespace};
            return route;
        }

        public static string GetRouteName(this IHttpRoute route)
        {
            return route.DataTokens["UR_RouteName"].ToString();
        }


    }

}