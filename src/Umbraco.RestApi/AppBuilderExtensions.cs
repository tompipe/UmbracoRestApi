using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Builder;
using Owin;
using Umbraco.RestApi.Controllers;

namespace Umbraco.RestApi
{
    public static class AppBuilderExtensions
    {
        public static void ConfigureUmbracoRestApi(this IAppBuilder app, UmbracoRestApiOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");
            UmbracoRestApiOptionsInstance.Options = options;
        }
    }
}
