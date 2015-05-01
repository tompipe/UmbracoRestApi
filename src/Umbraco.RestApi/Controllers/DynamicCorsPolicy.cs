using System.Web.Cors;

namespace Umbraco.RestApi.Controllers
{
    internal static class DynamicCorsPolicy
    {
        public static CorsPolicy CorsPolicy { get; set; }
    }
}