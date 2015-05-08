using System.Net.Http;
using System.Reflection;
using System.Web.Http;
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

            config.MapControllerAttributeRoutes(
                routeNamePrefix: "UmbracoRestApi",
                //Map these explicit controllers in the order they appear
                controllerTypes: new[]
                {                    
                    typeof (PublishedContentController),
                    typeof (ContentController)
                },
                routeCallback: route =>
                {

                },
                inheritedAttributes: true);

            //config.MapControllerAttributeRoutes(
            //   routeName: "UmbracoRestApi2",
            //    //Map these explicit controllers in the order they appear
            //   controllerTypes: new[]
            //    {                    
            //        typeof (ContentController)
            //    },
            //   routeCallback: route =>
            //   {

            //   },
            //   inheritedAttributes: true);

            //config.MapHttpAttributeRoutes(new CustomRouteAttributeDirectRouteProvider(true));

            ////HAL routes:

            ////** PublishedContent routes
            //MapEntityTypeRoute(config,
            //    RouteConstants.PublishedContentRouteName,
            //    string.Format("{0}/{1}/{2}", RouteConstants.GetRestRootPath(version), RouteConstants.ContentSegment, RouteConstants.PublishedSegment),
            //    "PublishedContent",
            //    typeof(ContentController).Namespace);

            ////** Content routes
            //MapEntityTypeRoute(config,
            //    RouteConstants.ContentRouteName,
            //    string.Format("{0}/{1}", RouteConstants.GetRestRootPath(version), RouteConstants.ContentSegment),
            //    "Content",
            //    typeof(ContentController).Namespace);

            ////** Media routes
            //MapEntityTypeRoute(config,
            //    RouteConstants.MediaRouteName,
            //    string.Format("{0}/{1}", RouteConstants.GetRestRootPath(version), RouteConstants.MediaSegment),
            //    "Media",
            //    typeof(ContentController).Namespace);

            ////** Members routes
            //MapEntityTypeRoute(config,
            //    RouteConstants.MembersRouteName,
            //    string.Format("{0}/{1}", RouteConstants.GetRestRootPath(version), RouteConstants.MembersSegment),
            //    "Members",
            //    typeof(ContentController).Namespace);


        }

        private static void MapEntityTypeRoute(HttpConfiguration config, string routeName, 
            string routeTemplateRoot, 
            string defaultController, 
            string @namespace)
        {
         
            

            ////route for search
            //var routeTemplateSearch = string.Concat(routeTemplateRoot.EnsureEndsWith('/'), "search");

            ////Used for search
            //config.Routes.MapHttpRouteWithNamespaceAndRouteName(config,
            //    name: RouteConstants.GetRouteNameForSearchRequests(routeName),
            //    routeTemplate: routeTemplateSearch,
            //    defaults: new {controller = defaultController, action = "Search"},
            //    constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get), controller = defaultController },
            //    @namespace: @namespace
            //    );

            ////template for Id + action routes
            //var routeTemplateIdGet = string.Concat(routeTemplateRoot.EnsureEndsWith('/'), "{id}/{action}");

            ////Used for 'GET' with Id + Action 
            //config.Routes.MapHttpRouteWithNamespaceAndRouteName(config,
            //    name: RouteConstants.GetRouteNameForIdGetRequests(routeName),
            //    routeTemplate: routeTemplateIdGet,
            //    defaults: new { controller = defaultController, action = "Get" },
            //    constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get), controller = defaultController },
            //    @namespace: @namespace
            //    );

            ////standard web.api template route for POST, DELETE, empty GET, etc...
            //var routeTemplateDefault = string.Concat(routeTemplateRoot.EnsureEndsWith('/'), "{*allvalues}");

            ////Used for everything else (POST, DELETE, etc...)
            //config.Routes.MapHttpRouteWithNamespaceAndRouteName(config,
            //    name: routeName,
            //    routeTemplate: routeTemplateDefault,
            //    defaults: new {controller = defaultController, action = "Get"},
            //    constraints: new { controller = defaultController },
            //    @namespace: @namespace
            //    );
        }

        
    }
}
