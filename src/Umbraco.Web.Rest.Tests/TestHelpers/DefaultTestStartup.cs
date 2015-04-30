using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using AutoMapper;
using Owin;
using Umbraco.Core.Services;
using Umbraco.Web.Rest.Models;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Rest.Tests.TestHelpers
{
    /// <summary>
    /// OWIN startup class for the self-hosted web server
    /// </summary>
    public class DefaultTestStartup
    {
        private readonly Action<HttpRequestMessage, UmbracoContext, ITypedPublishedContentQuery, ServiceContext> _activator;

        public DefaultTestStartup(Action<HttpRequestMessage, UmbracoContext, ITypedPublishedContentQuery, ServiceContext> activator)
        {
            _activator = activator;
        }

        private void Activator(HttpRequestMessage httpRequestMessage, UmbracoContext umbracoContext, ITypedPublishedContentQuery arg3, ServiceContext serviceContext)
        {
            _activator(httpRequestMessage, umbracoContext, arg3, serviceContext);

            Mapper.Initialize(configuration =>
            {
                var contentRepresentationMapper = new ContentRepresentationMapper();
                contentRepresentationMapper.ConfigureMappings(configuration, umbracoContext.Application);
            });
        }

        public void Configuration(IAppBuilder app)
        {

            var httpConfig = new HttpConfiguration();

            httpConfig.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            httpConfig.Services.Replace(typeof(IAssembliesResolver), new TestWebApiResolver());
            httpConfig.Services.Replace(typeof(IHttpControllerActivator), new DefaultTestControllerActivator(Activator));
            httpConfig.Services.Replace(typeof(IHttpControllerSelector), new NamespaceHttpControllerSelector(httpConfig));

            //auth everything
            app.AuthenticateEverything();

            //Create routes

            UmbracoRestStartup.CreateRoutes(httpConfig);

            app.UseWebApi(httpConfig);

        }
    }

}