using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

using Umbraco.Core;

namespace Umbraco.Web.Rest
{

    //TODO: Maybe take this out!! Developers will want to handle booting up their own webapi and they might be doing it with OWIN
    public class Startup : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            GlobalConfiguration.Configure(configuration =>
            {
                // Attribute routing.
                configuration.MapHttpAttributeRoutes();    
            });
        }
    }
}
