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
            CorsPolicy = new CorsPolicy()
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true,
                AllowAnyOrigin = true
            };
        }

        public CorsPolicy CorsPolicy { get; set; }
    }
}