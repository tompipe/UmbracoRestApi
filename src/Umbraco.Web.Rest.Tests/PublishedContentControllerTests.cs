using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Web.Rest.Routing;
using Umbraco.Web.Rest.Tests.TestHelpers;

namespace Umbraco.Web.Rest.Tests
{
    [TestFixture]
    public class PublishedContentControllerTests
    {
        [TestFixtureSetUp]
        public void TearDown()
        {
            ConfigurationManager.AppSettings.Set("umbracoPath", "~/umbraco");
            ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", UmbracoVersion.Current.ToString(3));
        }

        [Test]
        public async void Get_Children_Is_200_Response()
        {
            var startup = new DefaultTestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, contentService, mediaService, memberService) =>
                {
                    var mockTypedContent = Mock.Get(typedContent);
                    mockTypedContent.Setup(x => x.TypedContent(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedPublishedContent(123, 456, 789));
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/{1}/123/children", RouteConstants.ContentSegment, RouteConstants.PublishedSegment)),
                    Method = HttpMethod.Get,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Test]
        public async void Get_Id_Result()
        {
            var startup = new DefaultTestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, contentService, mediaService, memberService) =>
                {
                    var mockTypedContent = Mock.Get(typedContent);
                    mockTypedContent.Setup(x => x.TypedContent(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedPublishedContent(123, 456, 789));
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/{1}/123", RouteConstants.ContentSegment, RouteConstants.PublishedSegment)),
                    Method = HttpMethod.Get,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
                
                Assert.AreEqual("application/hal+json", result.Content.Headers.ContentType.MediaType);
                Assert.IsAssignableFrom<StreamContent>(result.Content);
                
                var djson = JsonConvert.DeserializeObject<JObject>(json);

                //TODO: Need to assert more values!
                //Assert.AreEqual("http://testserver/umbraco/rest/v1/Content/published", djson["collection"]["href"].Value<string>());
                //Assert.AreEqual(1, djson["collection"]["items"].Count());
                //Assert.AreEqual("http://testserver/umbraco/rest/v1/Content/published/123", djson["collection"]["items"][0]["href"].Value<string>());
                //Assert.AreEqual(12, djson["collection"]["items"][0]["data"].Count());

                //Assert.IsNotNull(djson["collection"]["items"][0]["data"].SingleOrDefault(x => x[FieldNames.Name].Value<string>() == FieldNames.Properties));
                //Assert.AreEqual(2, djson["collection"]["items"][0]["data"].SingleOrDefault(x => x[FieldNames.Name].Value<string>() == FieldNames.Properties)["array"].Count());

                //Assert.AreEqual(2, djson["collection"]["items"][0]["links"].Count());

                //Assert.AreEqual("children", djson["collection"]["items"][0]["links"][0]["rel"].Value<string>());
                //Assert.AreEqual("parent", djson["collection"]["items"][0]["links"][1]["rel"].Value<string>());
                //Assert.AreEqual("Children", djson["collection"]["items"][0]["links"][0]["prompt"].Value<string>());
                //Assert.AreEqual("Parent", djson["collection"]["items"][0]["links"][1]["prompt"].Value<string>());
                //Assert.AreEqual("http://testserver/umbraco/rest/v1/Content/published/123/children", djson["collection"]["items"][0]["links"][0]["href"].Value<string>());
                //Assert.AreEqual("http://testserver/umbraco/rest/v1/Content/published/456", djson["collection"]["items"][0]["links"][1]["href"].Value<string>());

            }
        }

        [Test]
        public async void Get_Empty_Is_200_Response()
        {
            var startup = new DefaultTestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services,
                (request, umbCtx, typedContent, contentService, mediaService, memberService) =>
                {
                    var mockTypedContent = Mock.Get(typedContent);
                    mockTypedContent.Setup(x => x.TypedContentAtRoot()).Returns(Enumerable.Empty<IPublishedContent>());
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/{1}", RouteConstants.ContentSegment, RouteConstants.PublishedSegment)),
                    Method = HttpMethod.Get,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);                
            }
        }

        [Test]
        public async void Post_Is_501_Response()
        {
            var startup = new DefaultTestStartup((request, umbCtx, typedContent, contentService, mediaService, memberService) => { });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/{1}", RouteConstants.ContentSegment, RouteConstants.PublishedSegment)),
                    Method = HttpMethod.Post,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                //NOTE: NotImplemented because we cannot post for published content
                Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
            }
        }

        [Test]
        public async void Put_Is_501_Response()
        {
            var startup = new DefaultTestStartup((request, umbCtx, typedContent, contentService, mediaService, memberService) => { });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/{1}/123", RouteConstants.ContentSegment, RouteConstants.PublishedSegment)),
                    Method = HttpMethod.Put,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                //NOTE: NotImplemented because we cannot post for published content
                Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
            }
        }

        [Test]
        public async void Delete_Is_501_Response()
        {
            var startup = new DefaultTestStartup((request, umbCtx, typedContent, contentService, mediaService, memberService) => { });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/{1}/123", RouteConstants.ContentSegment, RouteConstants.PublishedSegment)),
                    Method = HttpMethod.Delete,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                //NOTE: NotImplemented because we cannot post for published content
                Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);

            }
        }

        

    }
}

