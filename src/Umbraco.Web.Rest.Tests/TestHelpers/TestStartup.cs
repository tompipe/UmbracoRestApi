using System.Web.Http;
using System.Web.Http.Dispatcher;
using Owin;

namespace Umbraco.Web.Rest.Tests.TestHelpers
{
    /// <summary>
    /// OWIN startup class for the self-hosted web server
    /// </summary>
    public class TestStartup
    {
        public void Configuration(IAppBuilder app)
        {
            var configuration = new HttpConfiguration();

            configuration.Services.Replace(typeof(IAssembliesResolver), new TestWebApiResolver());
            configuration.Services.Replace(typeof(IHttpControllerActivator), new TestControllerActivator());

            //auth everything
            app.AuthenticateEverything();

            // Attribute routing.
            configuration.MapHttpAttributeRoutes();
            
            app.UseWebApi(configuration);
        }
    }
}