using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Examine;
using Examine.Providers;
using Examine.SearchCriteria;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.RestApi.Routing;
using Umbraco.RestApi.Tests.TestHelpers;

namespace Umbraco.RestApi.Tests
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
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, serviceContext, searchProvider) =>
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
        public async void Search_200_Result()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, serviceContext, searchProvider) =>
                {
                    var mockTypedContent = Mock.Get(typedContent);
                    mockTypedContent.Setup(x => x.TypedSearch(It.IsAny<ISearchCriteria>(), It.IsAny<BaseSearchProvider>()))
                        .Returns(new[]
                        {
                            ModelMocks.SimpleMockedPublishedContent(123, -1, 789),
                            ModelMocks.SimpleMockedPublishedContent(456, -1, 321)
                        });

                    var mockSearchProvider = Mock.Get(searchProvider);
                    mockSearchProvider.Setup(x => x.CreateSearchCriteria()).Returns(Mock.Of<ISearchCriteria>());                  

                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/{1}/search?lucene=parentID:\\-1", RouteConstants.ContentSegment, RouteConstants.PublishedSegment)),
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
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, serviceContext, searchProvider) =>
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
                
                //TODO: Need to assert more values!

                var djson = JsonConvert.DeserializeObject<JObject>(json);

                Assert.AreEqual("/umbraco/rest/v1/content/published/123", djson["_links"]["self"]["href"].Value<string>());
                Assert.AreEqual("/umbraco/rest/v1/content/published/456", djson["_links"]["parent"]["href"].Value<string>());
                Assert.AreEqual("/umbraco/rest/v1/content/published/{id}/children{?pageIndex,pageSize}", djson["_links"]["children"]["href"].Value<string>());
                Assert.AreEqual("/umbraco/rest/v1/content/published", djson["_links"]["root"]["href"].Value<string>());

                var properties = djson["properties"].ToObject<IDictionary<string, object>>();
                Assert.AreEqual(2, properties.Count());
                Assert.IsTrue(properties.ContainsKey("TestProperty1"));
                Assert.IsTrue(properties.ContainsKey("testProperty2"));
            }
        }

        [Test]
        public async void Get_Root_With_OPTIONS()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services,
                (request, umbCtx, typedContent, serviceContext, searchProvider) =>
                {
                    var mockTypedContent = Mock.Get(typedContent);
                    mockTypedContent.Setup(x => x.TypedContentAtRoot()).Returns(new[]
                    {
                        ModelMocks.SimpleMockedPublishedContent(123, -1, 789),
                        ModelMocks.SimpleMockedPublishedContent(456, -1, 321)
                    });
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/{1}", RouteConstants.ContentSegment, RouteConstants.PublishedSegment)),
                    Method = HttpMethod.Get,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                request.Headers.Add("Access-Control-Request-Headers", "accept, authorization");
                request.Headers.Add("Access-Control-Request-Method", "GET");
                request.Headers.Add("Origin", "http://localhost:12061");
                request.Headers.Add("Referer", "http://localhost:12061/browser.html");

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Test]
        public async void Get_Root_Result()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services,
                (request, umbCtx, typedContent, serviceContext, searchProvider) =>
                {
                    var mockTypedContent = Mock.Get(typedContent);
                    mockTypedContent.Setup(x => x.TypedContentAtRoot()).Returns(new[]
                    {
                        ModelMocks.SimpleMockedPublishedContent(123, -1, 789),
                        ModelMocks.SimpleMockedPublishedContent(456, -1, 321)
                    });
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

                var djson = JsonConvert.DeserializeObject<JObject>(json);

                Assert.AreEqual("/umbraco/rest/v1/content/published", djson["_links"]["root"]["href"].Value<string>());
                Assert.AreEqual(2, djson["totalResults"].Value<int>());
                Assert.AreEqual(2, djson["_links"]["content"].Count());
                Assert.AreEqual(2, djson["_embedded"]["content"].Count()); 
            }
        }

        [Test]
        public async void Post_Is_501_Response()
        {
            var startup = new TestStartup((request, umbCtx, typedContent, serviceContext, searchProvider) => { });

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
            var startup = new TestStartup((request, umbCtx, typedContent, serviceContext, searchProvider) => { });

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
            var startup = new TestStartup((request, umbCtx, typedContent, serviceContext, searchProvider) => { });

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

