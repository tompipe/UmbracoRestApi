using System.Collections.Concurrent;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.Routing;
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

            CreateRoutes(RouteTable.Routes);
        }

        public static void CreateRoutes(RouteCollection routes)
        {
            //** PublishedContent routes
            MapEntityTypeRoute(routes,
                RouteConstants.PublishedContentRouteName,
                string.Format("{0}/rest/v1/{1}/{2}/{{id}}/{{action}}", UmbracoMvcArea, RouteConstants.ContentSegment, RouteConstants.PublishedSegment),
                string.Format("{0}/rest/v1/{1}/{2}/{{id}}", UmbracoMvcArea, RouteConstants.ContentSegment, RouteConstants.PublishedSegment),
                "PublishedContent");

            //** Content routes
            MapEntityTypeRoute(routes,
                RouteConstants.ContentRouteName,
                string.Format("{0}/rest/v1/{1}/{{id}}/{{action}}", UmbracoMvcArea, RouteConstants.ContentSegment),
                string.Format("{0}/rest/v1/{1}/{{id}}", UmbracoMvcArea, RouteConstants.ContentSegment),
                "Content");

            //** Media routes
            MapEntityTypeRoute(routes,
                RouteConstants.MediaRouteName,
                string.Format("{0}/rest/v1/{1}/{{id}}/{{action}}", UmbracoMvcArea, RouteConstants.MediaSegment),
                string.Format("{0}/v1/{1}/{{id}}", UmbracoMvcArea, RouteConstants.MediaSegment),
                "Media");

            //** Members routes
            MapEntityTypeRoute(routes,
                RouteConstants.MembersRouteName,
                string.Format("{0}/rest/v1/{1}/{{id}}/{{action}}", UmbracoMvcArea, RouteConstants.MembersSegment),
                string.Format("{0}/rest/v1/{1}/{{id}}", UmbracoMvcArea, RouteConstants.MembersSegment),
                "Members");
        }
        
        public static void MapEntityTypeRoute(RouteCollection routes, string routeName, string routeTemplateGet, string routeTemplateOther, string defaultController)
        {
            
            //Used for 'GETs' since we have multiple get action names
            routes.MapHttpRoute(
                name: RouteConstants.GetRouteNameForGetRequests(routeName),
                routeTemplate: routeTemplateGet,
                defaults: new { controller = defaultController, action = "Get", id = RouteParameter.Optional },
                constraints: new { httpMethod = new System.Web.Http.Routing.HttpMethodConstraint(HttpMethod.Get) }
                )
                .WithRouteName(routeName)
                .WithNamespace(typeof(PublishedContentController).Namespace);

            //Used for everything else
            routes.MapHttpRoute(
                name: routeName,
                routeTemplate: routeTemplateOther,
                defaults: new { controller = defaultController, id = RouteParameter.Optional }
                )
                .WithRouteName(routeName)
                .WithNamespace(typeof(PublishedContentController).Namespace);
        }

        private static string _umbracoMvcArea;

        /// <summary>
        /// This returns the string of the MVC Area route.
        /// </summary>
        /// <remarks>
        /// Uses reflection to get the internal property in umb core, we don't want to expose this publicly in the core
        /// until we sort out the Global configuration bits and make it an interface, put them in the correct place, etc...
        /// </remarks>
        private static string UmbracoMvcArea
        {
            get
            {
                return _umbracoMvcArea ??
                       //Use reflection to get the type and value and cache
                       (_umbracoMvcArea = (string) Assembly.Load("Umbraco.Core").GetType("Umbraco.Core.Configuration.GlobalSettings").GetStaticProperty("UmbracoMvcArea"));
            }
        }
    }
}
