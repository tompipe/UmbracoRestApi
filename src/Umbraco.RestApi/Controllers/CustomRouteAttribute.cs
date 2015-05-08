using System;
using System.Web.Http;

namespace Umbraco.RestApi.Controllers
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class CustomRouteAttribute : Attribute
    {
        public CustomRouteAttribute(string template)
        {
            InnerAttribute = new RouteAttribute(template);
        }
        public RouteAttribute InnerAttribute;
    }
}