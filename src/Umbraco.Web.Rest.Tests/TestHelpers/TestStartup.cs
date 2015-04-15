using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using CollectionJson;
using Owin;
using Umbraco.Core.Services;
using Umbraco.Web.Rest.Routing;

namespace Umbraco.Web.Rest.Tests.TestHelpers
{
    /// <summary>
    /// OWIN startup class for the self-hosted web server
    /// </summary>
    public class TestStartup<TItem>
    {

        const string UmbracoMvcArea = "umbraco";
        private readonly Func<HttpRequestMessage, UmbracoContext, ITypedPublishedContentQuery, IContentService, IMediaService, IMemberService, Tuple<ICollectionJsonDocumentWriter<TItem>, ICollectionJsonDocumentReader<TItem>>> _activator;

        public TestStartup(Func<HttpRequestMessage, UmbracoContext, ITypedPublishedContentQuery, IContentService, IMediaService, IMemberService, Tuple<ICollectionJsonDocumentWriter<TItem>, ICollectionJsonDocumentReader<TItem>>> activator)
        {
            _activator = activator;
        }

        public void Configuration(IAppBuilder app)
        {
            var httpConfig = new HttpConfiguration();

            httpConfig.Services.Replace(typeof(IAssembliesResolver), new TestWebApiResolver());
            httpConfig.Services.Replace(typeof(IHttpControllerActivator), new TestControllerActivator<TItem>(_activator));

            //auth everything
            app.AuthenticateEverything();

            //Create routes

            var routes = httpConfig.Routes;

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
                string.Format("{0}/rest/v1/{1}/{{id}}", UmbracoMvcArea, RouteConstants.MediaSegment),
                "Media");

            //** Members routes
            MapEntityTypeRoute(routes,
                RouteConstants.MembersRouteName,
                string.Format("{0}/rest/v1/{1}/{{id}}/{{action}}", UmbracoMvcArea, RouteConstants.MembersSegment),
                string.Format("{0}/rest/v1/{1}/{{id}}", UmbracoMvcArea, RouteConstants.MembersSegment),
                "Members");

            app.UseWebApi(httpConfig);
        }

        private void MapEntityTypeRoute(HttpRouteCollection routes, string routeName, string routeTemplateGet, string routeTemplateOther, string defaultController)
        {
            //Used for 'GETs' since we have multiple get action names
            routes.MapHttpRoute(
                name: RouteConstants.GetRouteNameForGetRequests(routeName),
                routeTemplate: routeTemplateGet,
                defaults: new { controller = defaultController, action = "Get", id = RouteParameter.Optional },
                constraints: new { httpMethod = new System.Web.Http.Routing.HttpMethodConstraint(HttpMethod.Get) }
                ).WithRouteName(routeName);
            //Used for everything else
            routes.MapHttpRoute(
                name: routeName,
                routeTemplate: routeTemplateOther,
                defaults: new { controller = defaultController, id = RouteParameter.Optional }
                ).WithRouteName(routeName);
        }
    }
}