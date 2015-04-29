using System;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core.Services;
using Umbraco.Web.Rest.Controllers.OData;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Rest.Tests.TestHelpers
{
    public class DefaultTestControllerActivator : TestControllerActivatorBase
    {
        private readonly Action<HttpRequestMessage, UmbracoContext, ITypedPublishedContentQuery, IContentService, IMediaService, IMemberService> _onServicesCreated;

        public DefaultTestControllerActivator(Action<HttpRequestMessage, UmbracoContext, ITypedPublishedContentQuery, IContentService, IMediaService, IMemberService> onServicesCreated)
        {
            _onServicesCreated = onServicesCreated;
        }

        protected override ApiController CreateController(Type controllerType, HttpRequestMessage msg, UmbracoHelper helper, ITypedPublishedContentQuery qry, IContentService contentService, IMediaService mediaService, IMemberService memberService)
        {
            _onServicesCreated(msg, helper.UmbracoContext, qry, contentService, mediaService, memberService);

            //Create the controller with all dependencies
            var ctor = controllerType.GetConstructor(new[]
                {
                    typeof(UmbracoContext), 
                    typeof(UmbracoHelper)
                });

            if (ctor == null)
            {
                throw new MethodAccessException("Could not find the required constructor for the controller");
            }

            var created = (ApiController)ctor.Invoke(new object[]
                    {
                        //ctor args
                        helper.UmbracoContext, 
                        helper
                    });

            return created;
        }
    }
}