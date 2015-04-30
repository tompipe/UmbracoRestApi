using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Web.Rest.Models;
using Umbraco.Web.Rest.Routing;
using Umbraco.Web.Rest.Tests.TestHelpers;

namespace Umbraco.Web.Rest.Tests
{
    [TestFixture]
    public class ContentControllerTests
    {
        [TestFixtureSetUp]
        public void TearDown()
        {
            ConfigurationManager.AppSettings.Set("umbracoPath", "~/umbraco");
            ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", UmbracoVersion.Current.ToString(3));
            var mockSettings = MockUmbracoSettings.GenerateMockSettings();
            UmbracoConfig.For.CallMethod("SetUmbracoSettings", mockSettings);
        }


        [Test]
        public async void Get_Root_Result()
        {
            var startup = new DefaultTestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services,
                (request, umbCtx, typedContent, serviceContext) =>
                {
                    var mockContentService = Mock.Get(serviceContext.ContentService);
                    mockContentService.Setup(x => x.GetRootContent()).Returns(new[]
                    {
                        ModelMocks.SimpleMockedContent(123, -1),
                        ModelMocks.SimpleMockedContent(456, -1)
                    });

                    mockContentService.Setup(x => x.GetChildren(123)).Returns(new[] { ModelMocks.SimpleMockedContent(789, 123) });
                    mockContentService.Setup(x => x.GetChildren(456)).Returns(new[] { ModelMocks.SimpleMockedContent(321, 456) });
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}", RouteConstants.ContentSegment)),
                    Method = HttpMethod.Get,
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

                var djson = JsonConvert.DeserializeObject<JObject>(json);

                Assert.AreEqual("/umbraco/rest/v1/content", djson["_links"]["root"]["href"].Value<string>());
                Assert.AreEqual(2, djson["totalResults"].Value<int>());
                Assert.AreEqual(2, djson["_links"]["content"].Count());
                Assert.AreEqual(2, djson["_embedded"]["content"].Count()); 
            }
        }

        [Test]
        public async void Get_Id_Result()
        {
            var startup = new DefaultTestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                 (request, umbCtx, typedContent, serviceContext) =>
                 {
                     var mockContentService = Mock.Get(serviceContext.ContentService);

                     mockContentService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedContent());

                     mockContentService.Setup(x => x.GetChildren(It.IsAny<int>())).Returns(new List<IContent>(new[] { ModelMocks.SimpleMockedContent(789) }));

                     mockContentService.Setup(x => x.HasChildren(It.IsAny<int>())).Returns(true);
                 });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/123", RouteConstants.ContentSegment)),
                    Method = HttpMethod.Get,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

                //TODO: Assert values!

                var djson = JsonConvert.DeserializeObject<JObject>(json);

                Assert.AreEqual("/umbraco/rest/v1/content/123", djson["_links"]["self"]["href"].Value<string>());
                Assert.AreEqual("/umbraco/rest/v1/content/456", djson["_links"]["parent"]["href"].Value<string>());
                Assert.AreEqual("/umbraco/rest/v1/content/123/children", djson["_links"]["children"]["href"].Value<string>());
                Assert.AreEqual("/umbraco/rest/v1/content", djson["_links"]["root"]["href"].Value<string>());

                var properties = djson["properties"].ToObject<IDictionary<string, object>>();
                Assert.AreEqual(2, properties.Count()); 
            }
        }

        [Test]
        public async void Get_Metadata_Result()
        {
            var startup = new DefaultTestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                 (request, umbCtx, typedContent, serviceContext) =>
                 {
                     var mockContentService = Mock.Get(serviceContext.ContentService);

                     mockContentService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedContent());
                     mockContentService.Setup(x => x.GetChildren(It.IsAny<int>())).Returns(new List<IContent>(new[] { ModelMocks.SimpleMockedContent(789) }));
                     mockContentService.Setup(x => x.HasChildren(It.IsAny<int>())).Returns(true);

                     var mockTextService = Mock.Get(serviceContext.TextService);

                     mockTextService.Setup(x => x.Localize(It.IsAny<string>(), It.IsAny<CultureInfo>(), It.IsAny<IDictionary<string, string>>()))
                         .Returns((string input, CultureInfo culture, IDictionary<string, string> tokens) => input);
                 });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/123/meta", RouteConstants.ContentSegment)),
                    Method = HttpMethod.Get,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

                //TODO: Assert values!

               
            }
        }

        [Test]
        public async void Get_Children_Is_200_Response()
        {
            var startup = new DefaultTestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, serviceContext) =>
                {
                    var mockContentService = Mock.Get(serviceContext.ContentService);

                    mockContentService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedContent());

                    mockContentService.Setup(x => x.GetChildren(It.IsAny<int>())).Returns(new List<IContent>(new[] { ModelMocks.SimpleMockedContent(789) }));

                    mockContentService.Setup(x => x.HasChildren(It.IsAny<int>())).Returns(true);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/123/children", RouteConstants.ContentSegment)),
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
        public async void Post_Is_201_Response()
        {
            var startup = new DefaultTestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, serviceContext) =>
                {
                    var mockContentService = Mock.Get(serviceContext.ContentService);

                    mockContentService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedContent());

                    mockContentService.Setup(x => x.GetChildren(It.IsAny<int>())).Returns(new List<IContent>(new[] { ModelMocks.SimpleMockedContent(789) }));

                    mockContentService.Setup(x => x.HasChildren(It.IsAny<int>())).Returns(true);

                    mockContentService.Setup(x => x.CreateContent(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                        .Returns(() => ModelMocks.SimpleMockedContent(8888));
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}", RouteConstants.ContentSegment)),
                    Method = HttpMethod.Post,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                request.Content = new StringContent(@"{
  ""contentTypeAlias"": ""testType"",
  ""parentId"": 456,
  ""templateId"": 9,
  ""name"": ""Home"",
  ""properties"": {
    ""testProperty1"": {
      ""value"": ""property value1""
    },
    ""testProperty2"": {
      ""value"": ""property value2""
    }
  }
}", Encoding.UTF8, "application/json");

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            }
        }

        [Test]
        public async void Post_Is_400_Validation_Error()
        {
            var startup = new DefaultTestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, serviceContext) =>
                {
                    var mockContentService = Mock.Get(serviceContext.ContentService);

                    mockContentService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedContent());

                    mockContentService.Setup(x => x.GetChildren(It.IsAny<int>())).Returns(new List<IContent>(new[] { ModelMocks.SimpleMockedContent(789) }));

                    mockContentService.Setup(x => x.HasChildren(It.IsAny<int>())).Returns(true);

                    mockContentService.Setup(x => x.CreateContent(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                        .Returns(() => ModelMocks.SimpleMockedContent(8888));
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}", RouteConstants.ContentSegment)),
                    Method = HttpMethod.Post,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                request.Content = new StringContent(@"{
  ""contentTypeAlias"": """",
  ""parentId"": 456,
  ""templateId"": 9,
  ""name"": """",
  ""properties"": {
    ""testProperty1"": {
      ""value"": ""property value1""
    },
    ""testProperty2"": {
      ""value"": ""property value2""
    }
  }
}", Encoding.UTF8, "application/json");

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            }
        }

        [Test]
        public async void Put_Is_200_Response()
        {
            var startup = new DefaultTestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, serviceContext) =>
                {
                    var mockContentService = Mock.Get(serviceContext.ContentService);

                    mockContentService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedContent());

                    mockContentService.Setup(x => x.GetChildren(It.IsAny<int>())).Returns(new List<IContent>(new[] { ModelMocks.SimpleMockedContent(789) }));

                    mockContentService.Setup(x => x.HasChildren(It.IsAny<int>())).Returns(true);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/123", RouteConstants.ContentSegment)),
                    Method = HttpMethod.Put,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                request.Content = new StringContent(@"{
  ""contentTypeAlias"": ""testType"",
  ""parentId"": 456,
  ""templateId"": 9,
  ""name"": ""Home"",
  ""properties"": {
    ""testProperty1"": {
      ""value"": ""property value1""
    },
    ""testProperty2"": {
      ""value"": ""property value2""
    }
  }
}", Encoding.UTF8, "application/json");

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }
    }
}