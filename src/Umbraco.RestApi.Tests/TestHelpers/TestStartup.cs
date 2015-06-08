using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Dispatcher;
using AutoMapper;
using Examine.Providers;
using Owin;
using Umbraco.Core.Services;
using Umbraco.RestApi.Models;
using Umbraco.Web;
using Umbraco.Web.WebApi;

namespace Umbraco.RestApi.Tests.TestHelpers
{
    /// <summary>
    /// OWIN startup class for the self-hosted web server
    /// </summary>
    public class TestStartup
    {
        private readonly Action<HttpRequestMessage, UmbracoContext, ITypedPublishedContentQuery, ServiceContext, BaseSearchProvider> _activator;

        public TestStartup(Action<HttpRequestMessage, UmbracoContext, ITypedPublishedContentQuery, ServiceContext, BaseSearchProvider> activator)
        {
            _activator = activator;
        }

        private void Activator(HttpRequestMessage httpRequestMessage, UmbracoContext umbracoContext, ITypedPublishedContentQuery arg3, ServiceContext serviceContext, BaseSearchProvider searchProvider)
        {
            _activator(httpRequestMessage, umbracoContext, arg3, serviceContext, searchProvider);

            Mapper.Initialize(configuration =>
            {
                var contentRepresentationMapper = new ContentModelMapper();
                contentRepresentationMapper.ConfigureMappings(configuration, umbracoContext.Application);

                var mediaRepresentationMapper = new MediaModelMapper();
                mediaRepresentationMapper.ConfigureMappings(configuration, umbracoContext.Application);

                var memberRepresentationMapper = new MemberModelMapper();
                memberRepresentationMapper.ConfigureMappings(configuration, umbracoContext.Application);

                var relationRepresentationMapper = new RelationModelMapper();
                relationRepresentationMapper.ConfigureMappings(configuration, umbracoContext.Application);
            });
        }

        public void Configuration(IAppBuilder app)
        {

            var httpConfig = new HttpConfiguration();

            //this is here to ensure that multiple calls to this don't cause errors
            //httpConfig.MapHttpAttributeRoutes();
            
            httpConfig.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            httpConfig.Services.Replace(typeof(IAssembliesResolver), new SpecificAssemblyResolver(new[] { typeof(UmbracoRestStartup).Assembly }));
            httpConfig.Services.Replace(typeof(IHttpControllerActivator), new TestControllerActivator(Activator));
            httpConfig.Services.Replace(typeof(IHttpControllerSelector), new NamespaceHttpControllerSelector(httpConfig));

            //auth everything
            app.AuthenticateEverything();

            //Create routes

            UmbracoRestStartup.CreateRoutes(httpConfig);

            app.UseWebApi(httpConfig);
        }
    }

}