using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.Routing;

namespace Umbraco.RestApi
{
    internal static class RouteExtensions
    {
        public static IHttpRoute MapHttpRouteWithNamespaceAndRouteName(this HttpRouteCollection routes,
            string name, string routeTemplate, object defaults, object constraints,
            string @namespace)
        {
            var defaultsDictionary = new HttpRouteValueDictionary(defaults);
            var constraintsDictionary = new HttpRouteValueDictionary(constraints);
            var route = routes.CreateRoute(routeTemplate, defaultsDictionary, constraintsDictionary, 
                dataTokens: new Dictionary<string, object>(), //ensure there's data tokens
                handler: null);
            routes.Add(name, route);
            route.WithNamespace(@namespace);
            route.WithRouteName(name);
            return route;
        }

        public static IHttpRoute MapHttpRouteWithNamespaceAndRouteName(this HttpRouteCollection routes,
            string name, string routeTemplate, object defaults,
            string @namespace)
        {
            var defaultsDictionary = new HttpRouteValueDictionary(defaults);
            var constraintsDictionary = new HttpRouteValueDictionary();
            var route = routes.CreateRoute(routeTemplate, defaultsDictionary, constraintsDictionary,
                dataTokens: new Dictionary<string, object>(), //ensure there's data tokens
                handler: null);
            routes.Add(name, route);
            route.WithNamespace(@namespace);
            route.WithRouteName(name);
            return route;
        }

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