using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Batch;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.OData;
using System.Web.OData.Batch;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using System.Web.Routing;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace Umbraco.Web.Rest
{
    internal static class RouteExtensions
    {
        public static Route WithRouteName(this Route route, string routeName)
        {
            if (route.DataTokens == null)
            {
                route.DataTokens = new RouteValueDictionary();
            }
            route.DataTokens["UR_RouteName"] = routeName;
            return route;
        }

        public static IHttpRoute WithRouteName(this IHttpRoute route, string routeName)
        {
            route.DataTokens["UR_RouteName"] = routeName;
            return route;
        }

        public static Route WithNamespace(this Route route, string @namespace)
        {
            if (route.DataTokens == null)
            {
                route.DataTokens = new RouteValueDictionary();
            }
            route.DataTokens["Namespaces"] = new string[] {@namespace};
            ;
            return route;
        }

        public static IHttpRoute WithNamespace(this IHttpRoute route, string @namespace)
        {
            route.DataTokens["Namespaces"] = new string[] {@namespace};
            return route;
        }

        public static string GetRouteName(this IHttpRoute route)
        {
            return route.DataTokens["UR_RouteName"].ToString();
        }


    }

    //public class CustomAttributeRoutingConvention : AttributeRoutingConvention
    //{
    //    private readonly List<Type> _controllers = new List<Type> {typeof (MetadataController)};

    //    public CustomAttributeRoutingConvention(IEdmModel model, HttpConfiguration configuration, IEnumerable<Type> controllers)
    //        : base(model, configuration)
    //    {
    //        _controllers.AddRange(controllers);
    //    }

    //    public override bool ShouldMapController(HttpControllerDescriptor controller)
    //    {

    //        return _controllers.Contains(controller.ControllerType);
    //    }
    //}

    //public static class HttpConfigExt
    //{
    //    public static ODataRoute CustomMapODataServiceRoute(this HttpConfiguration configuration, string routeName,
    //        string routePrefix, IEdmModel model, IEnumerable<Type> controllers)
    //    {
    //        var routingConventions = ODataRoutingConventions.CreateDefault();
    //        routingConventions.Insert(0, new CustomAttributeRoutingConvention(model, configuration, controllers));
    //        return configuration.MapODataServiceRoute(routeName, routePrefix, model, new DefaultODataPathHandler(),
    //            routingConventions);
    //    }
    //}
}