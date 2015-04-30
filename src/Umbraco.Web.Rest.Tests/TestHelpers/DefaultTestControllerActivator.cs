using System;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core.Services;

namespace Umbraco.Web.Rest.Tests.TestHelpers
{
    public class DefaultTestControllerActivator : TestControllerActivatorBase
    {
        private readonly Action<HttpRequestMessage, UmbracoContext, ITypedPublishedContentQuery, ServiceContext> _onServicesCreated;

        public DefaultTestControllerActivator(Action<HttpRequestMessage, UmbracoContext, ITypedPublishedContentQuery, ServiceContext> onServicesCreated)
        {
            _onServicesCreated = onServicesCreated;
        }

        protected override ApiController CreateController(Type controllerType, HttpRequestMessage msg, UmbracoHelper helper, ITypedPublishedContentQuery qry, ServiceContext serviceContext)
        {
            _onServicesCreated(msg, helper.UmbracoContext, qry, serviceContext);

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