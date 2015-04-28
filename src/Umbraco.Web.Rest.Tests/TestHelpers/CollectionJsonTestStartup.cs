using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
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
    public class CollectionJsonTestStartup<TItem>
    {        
        private readonly Func<HttpRequestMessage, UmbracoContext, ITypedPublishedContentQuery, IContentService, IMediaService, IMemberService, Tuple<ICollectionJsonDocumentWriter<TItem>, ICollectionJsonDocumentReader<TItem>>> _activator;

        public CollectionJsonTestStartup(Func<HttpRequestMessage, UmbracoContext, ITypedPublishedContentQuery, IContentService, IMediaService, IMemberService, Tuple<ICollectionJsonDocumentWriter<TItem>, ICollectionJsonDocumentReader<TItem>>> activator)
        {
            _activator = activator;
        }

        public void Configuration(IAppBuilder app)
        {
            var httpConfig = new HttpConfiguration();

            httpConfig.Services.Replace(typeof(IAssembliesResolver), new TestWebApiResolver());
            httpConfig.Services.Replace(typeof(IHttpControllerActivator), new CollectionJsonTestControllerActivator<TItem>(_activator));
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