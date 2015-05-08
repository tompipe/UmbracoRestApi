using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Routing;

namespace Umbraco.RestApi.Routing
{

    /// <summary>
    /// A custom route that wraps any other route but makes it writeable: DataTokens and Handler
    /// </summary>
    public class WriteableRoute : IHttpRoute
    {
        private readonly IHttpRoute _innerRoute;
        private IDictionary<string, object> _dataTokens;
        private HttpMessageHandler _handler;

        public WriteableRoute(IHttpRoute innerRoute)
        {
            _innerRoute = innerRoute;
        }

        public virtual IHttpRouteData GetRouteData(string virtualPathRoot, HttpRequestMessage request)
        {
            var result = _innerRoute.GetRouteData(virtualPathRoot, request);
            return result == null ? null : new HttpRouteData(this, new HttpRouteValueDictionary(result.Values));
        }

        public IHttpVirtualPathData GetVirtualPath(HttpRequestMessage request, IDictionary<string, object> values)
        {
            return _innerRoute.GetVirtualPath(request, values);
        }

        public string RouteTemplate
        {
            get { return _innerRoute.RouteTemplate; }
        }

        public IDictionary<string, object> Defaults
        {
            get { return _innerRoute.Defaults; }
        }

        public IDictionary<string, object> Constraints
        {
            get { return _innerRoute.Constraints; }
        }

        public IDictionary<string, object> DataTokens
        {
            get { return _innerRoute.DataTokens ?? _dataTokens; }
            set { _dataTokens = value; }
        }

        public HttpMessageHandler Handler
        {
            get { return _innerRoute.Handler ?? _handler; }
            set { _handler = value; }
        }
    }
}