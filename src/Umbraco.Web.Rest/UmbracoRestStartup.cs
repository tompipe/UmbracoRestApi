using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

using System.Web.Http.Routing;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Web.Rest.Controllers;
using Umbraco.Web.Rest.Controllers.CollectionJson;
using Umbraco.Web.Rest.Models;
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
            //OData routes:

            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<GenericContent>("Content");
            var odataRoute = config.MapODataServiceRoute(
                routeName: RouteConstants.PublishedContentRouteName + RouteConstants.ODataPrefix,
                routePrefix: string.Format("{0}/rest/v1/{1}", UmbracoMvcArea, RouteConstants.ODataPrefix),
                model: builder.GetEdmModel());

            //var constraint = (ODataPathRouteConstraint) odataRoute.Constraints.Single(x => x.Value is ODataPathRouteConstraint).Value;
            //odataRoute.Constraints.Clear();
            //odataRoute.Constraints.Add("Test", new TestODataPathRouteConstraint(constraint.PathHandler, constraint.EdmModel, constraint.RouteName, constraint.RoutingConventions));
            
            
            //odataRoute.Constraints.Remove(constraint);

            //config.CustomMapODataServiceRoute(
            //    routeName: RouteConstants.PublishedContentRouteName + RouteConstants.ODataPrefix,
            //    routePrefix: string.Format("{0}/rest/v1/{1}", UmbracoMvcArea, RouteConstants.ODataPrefix),
            //    model: builder.GetEdmModel(),
            //    controllers: new[] { typeof(Rest.Controllers.OData.ContentController) });

            //Collection+Json routes:

            //** PublishedContent routes
            MapEntityTypeRoute(config,
                RouteConstants.PublishedContentRouteName + RouteConstants.CollectionJsonPrefix,
                string.Format("{0}/rest/v1/{3}/{1}/{2}/{{id}}/{{action}}", UmbracoMvcArea, RouteConstants.ContentSegment, RouteConstants.PublishedSegment, RouteConstants.CollectionJsonPrefix),
                string.Format("{0}/rest/v1/{3}/{1}/{2}/{{id}}", UmbracoMvcArea, RouteConstants.ContentSegment, RouteConstants.PublishedSegment, RouteConstants.CollectionJsonPrefix),
                "PublishedContent");

            //** Content routes
            MapEntityTypeRoute(config,
                RouteConstants.ContentRouteName + RouteConstants.CollectionJsonPrefix,
                string.Format("{0}/rest/v1/{2}/{1}/{{id}}/{{action}}", UmbracoMvcArea, RouteConstants.ContentSegment, RouteConstants.CollectionJsonPrefix),
                string.Format("{0}/rest/v1/{2}/{1}/{{id}}", UmbracoMvcArea, RouteConstants.ContentSegment, RouteConstants.CollectionJsonPrefix),
                "Content");

            //** Media routes
            MapEntityTypeRoute(config,
                RouteConstants.MediaRouteName + RouteConstants.CollectionJsonPrefix,
                string.Format("{0}/rest/v1/{2}/{1}/{{id}}/{{action}}", UmbracoMvcArea, RouteConstants.MediaSegment, RouteConstants.CollectionJsonPrefix),
                string.Format("{0}/v1/{2}/{1}/{{id}}", UmbracoMvcArea, RouteConstants.MediaSegment, RouteConstants.CollectionJsonPrefix),
                "Media");

            //** Members routes
            MapEntityTypeRoute(config,
                RouteConstants.MembersRouteName + RouteConstants.CollectionJsonPrefix,
                string.Format("{0}/rest/v1/{2}/{1}/{{id}}/{{action}}", UmbracoMvcArea, RouteConstants.MembersSegment, RouteConstants.CollectionJsonPrefix),
                string.Format("{0}/rest/v1/{2}/{1}/{{id}}", UmbracoMvcArea, RouteConstants.MembersSegment, RouteConstants.CollectionJsonPrefix),
                "Members");

        }

        private static void MapEntityTypeRoute(HttpConfiguration config, string routeName, string routeTemplateGet, string routeTemplateOther, string defaultController)
        {

            //Used for 'GETs' since we have multiple get action names
            config.Routes.MapHttpRoute(
                name: RouteConstants.GetRouteNameForGetRequests(routeName),
                routeTemplate: routeTemplateGet,
                defaults: new { controller = defaultController, action = "Get", id = RouteParameter.Optional },
                constraints: new { httpMethod = new System.Web.Http.Routing.HttpMethodConstraint(HttpMethod.Get) }
                )
                .WithRouteName(routeName)
                .WithNamespace(typeof(PublishedContentController).Namespace);

            //Used for everything else
            config.Routes.MapHttpRoute(
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
                       (_umbracoMvcArea = (string)Assembly.Load("Umbraco.Core").GetType("Umbraco.Core.Configuration.GlobalSettings").GetStaticProperty("UmbracoMvcArea"));
            }
        }
    }
}
