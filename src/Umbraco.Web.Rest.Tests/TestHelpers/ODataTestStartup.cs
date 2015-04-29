using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using AutoMapper;
using CollectionJson;
using Owin;
using Umbraco.Core.Services;
using Umbraco.Web.Rest.Models;

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

        private void Activator(HttpRequestMessage httpRequestMessage, UmbracoContext umbracoContext, ITypedPublishedContentQuery arg3, IContentService arg4, IMediaService arg5, IMemberService arg6)
        {
            _activator(httpRequestMessage, umbracoContext, arg3, arg4, arg5, arg6);

            Mapper.Initialize(configuration =>
            {
                var mapper = new ODataContentMapper();
                mapper.ConfigureMappings(configuration, umbracoContext.Application);
            });
        }

        public void Configuration(IAppBuilder app)
        {
            var httpConfig = new HttpConfiguration();

            httpConfig.Services.Replace(typeof(IAssembliesResolver), new TestWebApiResolver());
            httpConfig.Services.Replace(typeof(IHttpControllerActivator), new ODataTestControllerActivator(Activator));
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