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
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Web.Rest.Controllers;
using Umbraco.Web.Rest.Serialization;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Web.Rest.Tests.TestHelpers
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

                var owinContext = request.GetOwinContext();
                
                var appCtx = new ApplicationContext(CacheHelper.CreateDisabledCacheHelper());

                //httpcontext with an auth'd user
                var httpContext = Mock.Of<HttpContextBase>(http => http.User == owinContext.Authentication.User);
                //chuck it into the props since this is what MS does when hosted
                request.Properties["MS_HttpContext"] = httpContext;

                var backofficeIdentity = (UmbracoBackOfficeIdentity) owinContext.Authentication.User.Identity;

                var webSecurity = new Mock<WebSecurity>(null, null);
                webSecurity.Setup(x => x.CurrentUser)
                    .Returns(Mock.Of<IUser>(u => u.IsApproved == true
                        && u.IsLockedOut == false
                        && u.AllowedSections == backofficeIdentity.AllowedApplications
                        && u.Email == "admin@admin.com"
                        && u.Id == (int)backofficeIdentity.Id
                        && u.Language == "en"
                        && u.Name == backofficeIdentity.RealName
                        && u.StartContentId == backofficeIdentity.StartContentNode
                        && u.StartMediaId == backofficeIdentity.StartMediaNode
                        && u.Username == backofficeIdentity.Username));

                var umbCtx = UmbracoContext.EnsureContext(
                    //set the user of the HttpContext
                    httpContext,
                    appCtx,
                    webSecurity.Object,
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