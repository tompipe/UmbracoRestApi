using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using Umbraco.Core;
using Umbraco.RestApi.Controllers;
using Umbraco.RestApi.Routing;

namespace Umbraco.RestApi
{
    public class UmbracoRestStartup : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //Create routes for REST
            //NOTE : we are NOT using attribute routing! This is because there is no way to enable attribute routing against your own
            // assemblies with a custom DefaultDirectRouteProvider which we would require to implement inherited attribute routes. 
            // So we're just going to create the routes manually which is far less intruisive for an end-user developer since we don't
            // have to muck around with any startup logic.

            CreateRoutes(GlobalConfiguration.Configuration);
        }
        

        public static void CreateRoutes(HttpConfiguration config)
        {
            const int version = 1;

            //HAL routes:

            //** PublishedContent routes
            MapEntityTypeRoute(config,
                RouteConstants.PublishedContentRouteName,
                string.Format("{0}/{1}/{2}", RouteConstants.GetRestRootPath(version), RouteConstants.ContentSegment, RouteConstants.PublishedSegment),
                "PublishedContent",
                typeof(ContentController).Namespace);

            //** Content routes
            MapEntityTypeRoute(config,
                RouteConstants.ContentRouteName,
                string.Format("{0}/{1}", RouteConstants.GetRestRootPath(version), RouteConstants.ContentSegment),
                "Content",
                typeof(ContentController).Namespace);

            //** Media routes
            MapEntityTypeRoute(config,
                RouteConstants.MediaRouteName,
                string.Format("{0}/{1}", RouteConstants.GetRestRootPath(version), RouteConstants.MediaSegment),
                "Media",
                typeof(ContentController).Namespace);

            //** Members routes
            MapEntityTypeRoute(config,
                RouteConstants.MembersRouteName,
                string.Format("{0}/{1}", RouteConstants.GetRestRootPath(version), RouteConstants.MembersSegment),
                "Members",
                typeof(ContentController).Namespace);

          
        }

        private static void MapEntityTypeRoute(HttpConfiguration config, string routeName, 
            string routeTemplateRoot, 
            string defaultController, 
            string @namespace)
        {
            
            //route for search
            var routeTemplateSearch = string.Concat(routeTemplateRoot.EnsureEndsWith('/'), "search");

            //Used for search
            config.Routes.MapHttpRouteWithNamespaceAndRouteName(
                name: RouteConstants.GetRouteNameForSearchRequests(routeName),
                routeTemplate: routeTemplateSearch,
                defaults: new {controller = defaultController, action = "search"},
                constraints: new {httpMethod = new HttpMethodConstraint(HttpMethod.Get)},
                @namespace: @namespace
                );

            //template for Id + action routes
            var routeTemplateIdGet = string.Concat(routeTemplateRoot.EnsureEndsWith('/'), "{id}/{action}");

            //Used for 'GET' with Id + Action 
            config.Routes.MapHttpRouteWithNamespaceAndRouteName(
                name: RouteConstants.GetRouteNameForIdGetRequests(routeName),
                routeTemplate: routeTemplateIdGet,
                defaults: new { controller = defaultController, action = "Get", id = RouteParameter.Optional },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) },
                @namespace: @namespace
                );

            //standard web.api template route for POST, DELETE, empty GET, etc...
            var routeTemplateDefault = string.Concat(routeTemplateRoot.EnsureEndsWith('/'), "{id}");

            //Used for everything else (POST, DELETE, etc...)
            config.Routes.MapHttpRouteWithNamespaceAndRouteName(
                name: routeName,
                routeTemplate: routeTemplateDefault,
                defaults: new {controller = defaultController, id = RouteParameter.Optional},
                @namespace: @namespace
                );
        }

        
    }
}
