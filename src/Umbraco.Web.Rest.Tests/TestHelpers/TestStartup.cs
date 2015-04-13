using System;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Owin;
using Umbraco.Core.Services;

namespace Umbraco.Web.Rest.Tests.TestHelpers
{
    /// <summary>
    /// OWIN startup class for the self-hosted web server
    /// </summary>
    public class TestStartup
    {
        private readonly Action<ITypedPublishedContentQuery, IContentService, IMediaService, IMemberService> _activator;

        public TestStartup(Action<ITypedPublishedContentQuery, IContentService, IMediaService, IMemberService> activator)
        {
            _activator = activator;
        }

        public void Configuration(IAppBuilder app)
        {
            var httpConfig = new HttpConfiguration();
            httpConfig.Services.Replace(typeof(IAssembliesResolver), new TestWebApiResolver());
            httpConfig.Services.Replace(typeof(IHttpControllerActivator), new TestControllerActivator(_activator));

            //auth everything
            app.AuthenticateEverything();

            // Attribute routing.
            httpConfig.MapHttpAttributeRoutes();

            app.UseWebApi(httpConfig);
        }
    }
}