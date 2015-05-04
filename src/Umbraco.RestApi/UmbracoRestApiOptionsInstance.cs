using System.CodeDom;
using System.Web.Cors;

namespace Umbraco.RestApi
{
    internal static class UmbracoRestApiOptionsInstance
    {
        static UmbracoRestApiOptionsInstance()
        {
            Options = new UmbracoRestApiOptions();
        }
        public static UmbracoRestApiOptions Options { get; set; }
    }
}