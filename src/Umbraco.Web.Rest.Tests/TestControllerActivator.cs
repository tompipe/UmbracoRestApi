using System;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using CollectionJson;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models;
using Umbraco.Web.Rest.Controllers;
using Umbraco.Web.Rest.Serialization;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Web.Rest.Tests
{
    /// <summary>
    /// Custom activator to create instances of our controllers with an UmbracoContext
    /// </summary>
    public class TestControllerActivator : DefaultHttpControllerActivator, IHttpControllerActivator
    {
        IHttpController IHttpControllerActivator.Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            if (typeof (UmbracoCollectionJsonController).IsAssignableFrom(controllerType))
            {
                //create it with an UmbracoContext

                var appCtx = new ApplicationContext(CacheHelper.CreateDisabledCacheHelper());

                var umbCtx = UmbracoContext.EnsureContext(
                    Mock.Of<HttpContextBase>(),
                    appCtx,
                    new Mock<WebSecurity>(null, null).Object,
                    Mock.Of<IUmbracoSettingsSection>(),
                    Enumerable.Empty<IUrlProvider>(),
                    true); //replace it

                var ctor = controllerType.GetConstructor(new[] { typeof(UmbracoContext), typeof(ICollectionJsonDocumentWriter<IPublishedContent>) });
                if (ctor != null)
                {
                    return (IHttpController)ctor.Invoke(new object[] { umbCtx, new ContentDocumentWriter(request) });
                }
            }
            //default
            return base.Create(request, controllerDescriptor, controllerType);
        }
    }
}