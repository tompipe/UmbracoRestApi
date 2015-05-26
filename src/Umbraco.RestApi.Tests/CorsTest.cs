using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Cors;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.RestApi.Routing;
using Umbraco.RestApi.Tests.TestHelpers;

namespace Umbraco.RestApi.Tests
{
    [TestFixture]
    public class CorsTests
    {
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            ConfigurationManager.AppSettings.Set("umbracoPath", "~/umbraco");
            ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", UmbracoVersion.Current.ToString(3));
            var mockSettings = MockUmbracoSettings.GenerateMockSettings();
            UmbracoConfig.For.CallMethod("SetUmbracoSettings", mockSettings);
        }

        [TearDown]
        public void TearDown()
        {
            //Hack - because Reset is internal
            typeof (PropertyEditorResolver).CallStaticMethod("Reset", true);
        }

        [Test]
        public async void Default_Options_Allow_Any_Origin()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services,
                (request, umbCtx, typedContent, serviceContext, searchProvider) =>
                {
                    var mockContentService = Mock.Get(serviceContext.ContentService);
                    mockContentService.Setup(x => x.GetRootContent()).Returns(Enumerable.Empty<IContent>());
                });

            using (var server = TestServer.Create(builder =>
            {
                startup.Configuration(builder);

                //default options
                builder.ConfigureUmbracoRestApi(new UmbracoRestApiOptions());
            }))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}", RouteConstants.ContentSegment)),
                    Method = HttpMethod.Get,
                };
                //add the origin so Cors kicks in!
                request.Headers.Add("Origin", "http://localhost:12061");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent) result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

                Assert.IsTrue(result.Headers.Contains("Access-Control-Allow-Origin"));
                var acao = result.Headers.GetValues("Access-Control-Allow-Origin");
                Assert.AreEqual(1, acao.Count());

                //looks like the mvc cors default is to allow the request domain instea of *
                Assert.AreEqual("http://localhost:12061", acao.First());
            }
        }

        [Test]
        public async void Supports_Creds()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services,
                (request, umbCtx, typedContent, serviceContext, searchProvider) =>
                {
                    var mockContentService = Mock.Get(serviceContext.ContentService);
                    mockContentService.Setup(x => x.GetRootContent()).Returns(Enumerable.Empty<IContent>());
                });

            using (var server = TestServer.Create(builder =>
            {
                startup.Configuration(builder);

                //default options
                builder.ConfigureUmbracoRestApi(new UmbracoRestApiOptions
                {
                    CorsPolicy = new CorsPolicy()
                    {
                        AllowAnyOrigin = true,
                        SupportsCredentials = true
                    }
                });
            }))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}", RouteConstants.ContentSegment)),
                    Method = HttpMethod.Get,
                };
                //add the origin so Cors kicks in!
                request.Headers.Add("Origin", "http://localhost:12061");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

                Assert.IsTrue(result.Headers.Contains("Access-Control-Allow-Origin"));
                var acao = result.Headers.GetValues("Access-Control-Allow-Origin");
                Assert.AreEqual(1, acao.Count());
                Assert.AreEqual("http://localhost:12061", acao.First());
            }
        }

        [Test]
        public async void Supports_Post()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, serviceContext, searchProvider) =>
                {
                   TestHelpers.ContentServiceMocks.SetupMocksForPost(serviceContext);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}", RouteConstants.ContentSegment)),
                    Method = HttpMethod.Post,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                request.Headers.Add("Origin", "http://localhost:12061");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

                request.Content = new StringContent(@"{
  ""contentTypeAlias"": ""testType"",
  ""parentId"": 456,
  ""templateId"": 9,
  ""name"": ""Home"",
  ""properties"": {
    ""TestProperty1"": ""property value1"",
    ""testProperty2"": ""property value2""
  }
}", Encoding.UTF8, "application/json");

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                //CORS
                Assert.IsTrue(result.Headers.Contains("Access-Control-Allow-Origin"));
                var acao = result.Headers.GetValues("Access-Control-Allow-Origin");
                Assert.AreEqual(1, acao.Count());
                Assert.AreEqual("http://localhost:12061", acao.First());

                //Creation
                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            }
        }


    }
}
