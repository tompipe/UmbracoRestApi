using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Cors;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using System.Web.Routing;

namespace Umbraco.RestApi
{
    internal static class RouteExtensions
    {
        public static void MapPerAssemblyAttributeRoutes(
            this HttpConfiguration originalConfig, 
            string routeName,
            Assembly assemblyToScan,
            bool inheritedAttributes = false)
        {
            //new temp config to clone
            var tempConfig = new HttpConfiguration();
            
            tempConfig.Services.Replace(typeof(IAssembliesResolver), new SpecificAssemblyResolver(new[] { assemblyToScan }));
            tempConfig.MapHttpAttributeRoutes(new CustomRouteAttributeDirectRouteProvider(inheritedAttributes));

            var isInitialized = false;

            var originalInit = originalConfig.Initializer;
            originalConfig.Initializer = configuration =>
            {
                //Track a boolean because otherwise we'll end up in an infinite loop because
                // of the ctor of the HttpControllerDescriptor below will clone http config
                // per controller and invoke the original initializer, and we don't want to
                // initialize the clones again with the attribute routes.
                if (!isInitialized)
                {
                    isInitialized = true;

                    tempConfig.EnsureInitialized();

                    //get the route created
                    var attributeRoute = tempConfig.Routes.Single();

                    //attributeRoute.Handler = GetMessageHandler()


                    //cast to it's collection
                    var attributeRoutes = (IReadOnlyCollection<IHttpRoute>)attributeRoute;
                    //update each attribute route's action descriptor http configuration property
                    //to be the real configuration 
                    foreach (var httpRoute in attributeRoutes)
                    {
                        var descriptors = httpRoute.DataTokens["actions"] as IEnumerable<HttpActionDescriptor>;
                        if (descriptors != null)
                        {
                            foreach (var descriptor in descriptors)
                            {
                                descriptor.Configuration = configuration;
                                //NOTE: We are making a new instance of a HttpControllerDescriptor to force
                                // the descriptor to initialize again so that IControllerConfiguration executes.
                                descriptor.ControllerDescriptor = new HttpControllerDescriptor(
                                    configuration,
                                    descriptor.ControllerDescriptor.ControllerName,
                                    descriptor.ControllerDescriptor.ControllerType);
                            }
                        }
                    }

                    //now we need to add the route back to the main configuration
                    originalConfig.Routes.Add(
                        routeName,
                        attributeRoute);
                }

                originalInit(configuration);
            };
        }

        //public static IHttpRoute MapHttpRouteWithNamespaceAndRouteName(this HttpRouteCollection routes,
        //    HttpConfiguration config,
        //    string name, string routeTemplate, object defaults, object constraints,
        //    string @namespace)
        //{
        //    var defaultsDictionary = new HttpRouteValueDictionary(defaults);
        //    var constraintsDictionary = new HttpRouteValueDictionary(constraints);
        //    var route = routes.CreateRoute(routeTemplate, defaultsDictionary, constraintsDictionary, 
        //        dataTokens: new Dictionary<string, object>(), //ensure there's data tokens
        //        handler: GetMessageHandler(config));
        //    routes.Add(name, route);
        //    route.WithNamespace(@namespace);
        //    route.WithRouteName(name);
        //    return route;
        //}

        //public static IHttpRoute MapHttpRouteWithNamespaceAndRouteName(this HttpRouteCollection routes,
        //    HttpConfiguration config,
        //    string name, string routeTemplate, object defaults,
        //    string @namespace)
        //{
        //    var defaultsDictionary = new HttpRouteValueDictionary(defaults);
        //    var constraintsDictionary = new HttpRouteValueDictionary();
        //    var route = routes.CreateRoute(routeTemplate, defaultsDictionary, constraintsDictionary,
        //        dataTokens: new Dictionary<string, object>(), //ensure there's data tokens
        //        handler: GetMessageHandler(config));
        //    routes.Add(name, route);
        //    route.WithNamespace(@namespace);
        //    route.WithRouteName(name);
        //    return route;
        //}

        /// <summary>
        /// Gets a custom message handler that explicitly contains the CorsMessageHandler for use 
        /// with our RestApi routes.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static HttpMessageHandler GetMessageHandler(HttpConfiguration config)
        {
            // Create a message handler chain with an end-point.
            return HttpClientFactory.CreatePipeline(
                new HttpControllerDispatcher(config), 
                new DelegatingHandler[]
                {                   
                    //Explicitly include the CorsMessage handler!
                    // we're doing this so people don't have to do EnableCors() in their startup,
                    // we don't care about that, we always want to support Cors for the rest api
                    new CorsMessageHandler(config)
                });
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