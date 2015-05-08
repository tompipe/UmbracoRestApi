using System;
using System.Diagnostics.Contracts;
using System.Web.Http;
using System.Web.Http.Routing;

namespace Umbraco.RestApi.Controllers
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class CustomRouteAttribute : Attribute, IDirectRouteFactory
    {
        public CustomRouteAttribute(string template)
        {
            InnerAttribute = new RouteAttribute(template);
        }

        public string Name
        {
            get { return InnerAttribute.Name; }
            set { InnerAttribute.Name = value; }
        }

        public RouteAttribute InnerAttribute;

        RouteEntry IDirectRouteFactory.CreateRoute(DirectRouteFactoryContext context)
        {
            var result = ((IDirectRouteFactory) InnerAttribute).CreateRoute(context);
            //need to add this here so we can retrieve it later
            result.Route.DataTokens.Add("Umb_RouteName", Name);
            return result;
        }
    }
}