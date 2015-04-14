using System;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Security;
using CollectionJson;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Rest.Controllers;
using Umbraco.Web.Rest.Serialization;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Rest.Tests.TestHelpers
{
    /// <summary>
    /// Custom activator to create instances of our controllers with an UmbracoContext
    /// </summary>
    public class TestControllerActivator<TItem> : DefaultHttpControllerActivator, IHttpControllerActivator
    {
        private readonly Func<HttpRequestMessage, ITypedPublishedContentQuery, IContentService, IMediaService, IMemberService, Tuple<ICollectionJsonDocumentWriter<TItem>, ICollectionJsonDocumentReader<TItem>>> _onServicesCreated;

        public TestControllerActivator(Func<HttpRequestMessage, ITypedPublishedContentQuery, IContentService, IMediaService, IMemberService, Tuple<ICollectionJsonDocumentWriter<TItem>, ICollectionJsonDocumentReader<TItem>>> onServicesCreated)
        {
            _onServicesCreated = onServicesCreated;
        }

        IHttpController IHttpControllerActivator.Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            if (typeof(UmbracoApiControllerBase).IsAssignableFrom(controllerType))
            {
                var owinContext = request.GetOwinContext();

                //Create mocked services that we are going to pass to the callback for unit tests to modify
                // before passing these services to the main container objects
                var mockedTypedContent = Mock.Of<ITypedPublishedContentQuery>();
                var mockedContentService = Mock.Of<IContentService>();
                var mockedMediaService = Mock.Of<IMediaService>();
                var mockedMemberService = Mock.Of<IMemberService>();

                var readerWriter = _onServicesCreated(request, mockedTypedContent, mockedContentService, mockedMediaService, mockedMemberService);    
                
                //new app context
                var appCtx = ApplicationContext.EnsureContext(
                    new DatabaseContext(Mock.Of<IDatabaseFactory>(), Mock.Of<ILogger>(), Mock.Of<ISqlSyntaxProvider>(), "test"), 
                    //pass in mocked services
                    new ServiceContext(contentService:mockedContentService, mediaService:mockedMediaService, memberService:mockedMemberService), 
                    CacheHelper.CreateDisabledCacheHelper(), 
                    new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()),
                    true);

                //httpcontext with an auth'd user
                var httpContext = Mock.Of<HttpContextBase>(http => http.User == owinContext.Authentication.User);
                //chuck it into the props since this is what MS does when hosted
                request.Properties["MS_HttpContext"] = httpContext;

                var backofficeIdentity = (UmbracoBackOfficeIdentity) owinContext.Authentication.User.Identity;

                var webSecurity = new Mock<WebSecurity>(null, null);

                //mock CurrentUser
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

                //mock Validate
                webSecurity.Setup(x => x.ValidateCurrentUser())
                    .Returns(() => true);

                var umbCtx = UmbracoContext.EnsureContext(
                    //set the user of the HttpContext
                    httpContext,
                    appCtx,
                    webSecurity.Object,
                    Mock.Of<IUmbracoSettingsSection>(),
                    Enumerable.Empty<IUrlProvider>(),
                    true); //replace it

                var urlHelper = new Mock<IUrlProvider>();
                urlHelper.Setup(provider => provider.GetUrl(It.IsAny<UmbracoContext>(), It.IsAny<int>(), It.IsAny<Uri>(), It.IsAny<UrlProviderMode>()))
                    .Returns("/hello/world/1234");

                var membershipHelper = new MembershipHelper(umbCtx, Mock.Of<MembershipProvider>(), Mock.Of<RoleProvider>());

                var umbHelper = new UmbracoHelper(umbCtx,
                    Mock.Of<IPublishedContent>(),
                    mockedTypedContent,
                    Mock.Of<IDynamicPublishedContentQuery>(),
                    Mock.Of<ITagQuery>(),
                    Mock.Of<IDataTypeService>(),
                    new UrlProvider(umbCtx, new[]
                    {
                        urlHelper.Object
                    }, UrlProviderMode.Auto),
                    Mock.Of<ICultureDictionary>(),
                    Mock.Of<IUmbracoComponentRenderer>(),
                    membershipHelper);

                //Create the controller with all dependencies
                var ctor = controllerType.GetConstructor(new[]
                {
                    typeof(UmbracoContext), 
                    typeof(UmbracoHelper), 
                    typeof(ICollectionJsonDocumentWriter<TItem>),
                    typeof(ICollectionJsonDocumentReader<TItem>)
                });

                if (ctor == null)
                {
                    throw new MethodAccessException("Could not find the required constructor for the controller");
                }

                var created = (UmbracoApiControllerBase)ctor.Invoke(new object[]
                    {
                        //ctor args
                        umbCtx, 
                        umbHelper,
                        readerWriter.Item1,
                        readerWriter.Item2
                    });

                return created;
            }
            //default
            return base.Create(request, controllerDescriptor, controllerType);
        }

        
    }
}