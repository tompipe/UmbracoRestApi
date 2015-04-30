using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Routing;
using Umbraco.Core;
using Umbraco.Web.Rest.Controllers;
using Umbraco.Web.Rest.Routing;

namespace Umbraco.Web.Rest
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
                string.Format("{0}/{1}/{2}/{{id}}/{{action}}", RouteConstants.GetRestRootPath(version), RouteConstants.ContentSegment, RouteConstants.PublishedSegment),
                string.Format("{0}/{1}/{2}/{{id}}", RouteConstants.GetRestRootPath(version), RouteConstants.ContentSegment, RouteConstants.PublishedSegment),
                "PublishedContent",
                typeof(ContentController).Namespace);

            //** Content routes
            MapEntityTypeRoute(config,
                RouteConstants.ContentRouteName,
                string.Format("{0}/{1}/{{id}}/{{action}}", RouteConstants.GetRestRootPath(version), RouteConstants.ContentSegment),
                string.Format("{0}/{1}/{{id}}", RouteConstants.GetRestRootPath(version), RouteConstants.ContentSegment),
                "Content",
                typeof(ContentController).Namespace);

            //** Media routes
            MapEntityTypeRoute(config,
                RouteConstants.MediaRouteName,
                string.Format("{0}/{1}/{{id}}/{{action}}", RouteConstants.GetRestRootPath(version), RouteConstants.MediaSegment),
                string.Format("{0}/{1}/{{id}}", RouteConstants.GetRestRootPath(version), RouteConstants.MediaSegment),
                "Media",
                typeof(ContentController).Namespace);

            //** Members routes
            MapEntityTypeRoute(config,
                RouteConstants.MembersRouteName,
                string.Format("{0}/{1}/{{id}}/{{action}}", RouteConstants.GetRestRootPath(version), RouteConstants.MembersSegment),
                string.Format("{0}/{1}/{{id}}", RouteConstants.GetRestRootPath(version), RouteConstants.MembersSegment),
                "Members",
                typeof(ContentController).Namespace);

          
        }

        private static void MapEntityTypeRoute(HttpConfiguration config, string routeName, string routeTemplateGet, string routeTemplateOther, string defaultController, string @namespace)
        {

            //Used for 'GETs' since we have multiple get action names
            config.Routes.MapHttpRoute(
                name: RouteConstants.GetRouteNameForGetRequests(routeName),
                routeTemplate: routeTemplateGet,
                defaults: new { controller = defaultController, action = "Get", id = RouteParameter.Optional },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) }
                )
                .WithRouteName(routeName)
                .WithNamespace(@namespace);

            //Used for everything else
            config.Routes.MapHttpRoute(
                name: routeName,
                routeTemplate: routeTemplateOther,
                defaults: new { controller = defaultController, id = RouteParameter.Optional }
                )
                .WithRouteName(routeName)
                .WithNamespace(@namespace);
        }

        
    }
}
