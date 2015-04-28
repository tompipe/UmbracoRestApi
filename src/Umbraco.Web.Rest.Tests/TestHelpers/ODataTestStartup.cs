using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using CollectionJson;
using Owin;
using Umbraco.Core.Services;

namespace Umbraco.Web.Rest.Tests.TestHelpers
{
    /// <summary>
    /// OWIN startup class for the self-hosted web server
    /// </summary>
    public class ODataTestStartup
    {
        private readonly Action<HttpRequestMessage, UmbracoContext, ITypedPublishedContentQuery, IContentService, IMediaService, IMemberService> _activator;

        public ODataTestStartup(Action<HttpRequestMessage, UmbracoContext, ITypedPublishedContentQuery, IContentService, IMediaService, IMemberService> activator)
        {
            _activator = activator;
        }

        public void Configuration(IAppBuilder app)
        {
            var httpConfig = new HttpConfiguration();

            httpConfig.Services.Replace(typeof(IAssembliesResolver), new TestWebApiResolver());
            httpConfig.Services.Replace(typeof(IHttpControllerActivator), new ODataTestControllerActivator(_activator));
            //httpConfig.Services.Replace(typeof(IHttpControllerTypeResolver), new TestControllerTypeResolver());
            //httpConfig.Services.Replace(typeof(IHttpControllerSelector), new TestControllerSelector(httpConfig));

            //auth everything
            app.AuthenticateEverything();
            
            //Create routes

            UmbracoRestStartup.CreateRoutes(httpConfig);

            app.UseWebApi(httpConfig);
        }
    }
}