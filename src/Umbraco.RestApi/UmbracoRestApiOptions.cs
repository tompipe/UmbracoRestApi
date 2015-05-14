using System.Web.Cors;

namespace Umbraco.RestApi
{
    public class UmbracoRestApiOptions
    {
        /// <summary>
        /// Default options allows all request, CORS does not limit anything
        /// </summary>
        public UmbracoRestApiOptions()
        {
            //These are the defaults that we know work with auth and the REST API
            // but people can modify them if required.
            CorsPolicy = new CorsPolicy()
            {
                AllowAnyOrigin = true,
                SupportsCredentials = true,
                Headers = {"authorization", "accept"},
                Methods = {"GET", "POST", "DELETE", "PUT"}
            };
        }

        public CorsPolicy CorsPolicy { get; set; }
    }
}