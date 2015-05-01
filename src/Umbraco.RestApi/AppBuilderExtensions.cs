using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Cors;
using Microsoft.Owin.Builder;
using Owin;
using Umbraco.RestApi.Controllers;

namespace Umbraco.RestApi
{
    public static class AppBuilderExtensions
    {
        public static void ConfigureUmbracoRestApiCors(this AppBuilder app, CorsPolicy corsPolicy)
        {
            if (corsPolicy == null) throw new ArgumentNullException("corsPolicy");
            DynamicCorsPolicy.CorsPolicy = corsPolicy;
        }
    }
}
