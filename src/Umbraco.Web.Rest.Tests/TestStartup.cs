using System.Web.Http;
using System.Web.Http.Dispatcher;
using Owin;

namespace Umbraco.Web.Rest.Tests
{
    public class TestStartup
    {
        public void Configuration(IAppBuilder app)
        {
            var configuration = new HttpConfiguration();

            configuration.Services.Replace(typeof(IAssembliesResolver), new TestWebApiResolver());
            configuration.Services.Replace(typeof(IHttpControllerActivator), new TestControllerActivator());

            // Attribute routing.
            configuration.MapHttpAttributeRoutes();

            // Execute any other ASP.NET Web API-related initialization, i.e. IoC, authentication, logging, mapping, DB, etc.
            //ConfigureAuthPipeline(app);

            app.UseWebApi(configuration);
        }
    }
}