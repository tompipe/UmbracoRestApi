using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using CollectionJson;
using CollectionJson.Client;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Rest.Routing;
using Umbraco.Web.Rest.Serialization;
using Umbraco.Web.Rest.Tests.TestHelpers;
using Umbraco.Web.Routing;

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
        public async void Get_Children_Is_200_Response()
        {
            var startup = new TestStartup<IContent>(
                //This will be invoked before the controller is created so we can modify these mocked services
                // it needs to return the required reader/writer for the tests
                (request, umbCtx, typedContent, contentService, mediaService, memberService) =>
                {
                    var mockContentService = Mock.Get(contentService);
                    mockContentService.Setup(x => x.GetChildren(It.IsAny<int>())).Returns(Enumerable.Empty<IContent>());

                    return new Tuple<ICollectionJsonDocumentWriter<IContent>, ICollectionJsonDocumentReader<IContent>>(
                        new ContentDocumentWriter(request, umbCtx.UrlProvider,  mockContentService.Object),
                        null);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.GetAsync(
                    string.Format("http://testserver/umbraco/v1/{0}/123/children", RouteConstants.ContentSegment));
                Console.WriteLine(result);
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Test]
        public async void Get_Id_Result()
        {
            var startup = new TestStartup<IContent>(
                //This will be invoked before the controller is created so we can modify these mocked services
                // it needs to return the required reader/writer for the tests
                (request, umbCtx, typedContent, contentService, mediaService, memberService) =>
                {
                    var mockContentService = Mock.Get(contentService);

                    mockContentService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => SimpleMockedContent());

                    mockContentService.Setup(x => x.GetChildren(It.IsAny<int>())).Returns(new List<IContent>(new[] { SimpleMockedContent(789) }));

                    return new Tuple<ICollectionJsonDocumentWriter<IContent>, ICollectionJsonDocumentReader<IContent>>(
                        new ContentDocumentWriter(request, umbCtx.UrlProvider, mockContentService.Object),
                        null);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.GetAsync(
                    string.Format("http://testserver/umbraco/v1/{0}/123", RouteConstants.ContentSegment));
                Console.WriteLine(result);

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
                Assert.AreEqual("application/vnd.collection+json", result.Content.Headers.ContentType.MediaType);
                Assert.IsAssignableFrom<StreamContent>(result.Content);
                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Assert.IsTrue(json.Contains("\"collection\""));

                var djson = JsonConvert.DeserializeObject<JObject>(json);
                Assert.AreEqual("http://testserver/umbraco/v1/content", djson["collection"]["href"].Value<string>());
                Assert.AreEqual(1, djson["collection"]["items"].Count());
                Assert.AreEqual("http://testserver/umbraco/v1/content/123", djson["collection"]["items"][0]["href"].Value<string>());
                Assert.AreEqual(11, djson["collection"]["items"][0]["data"].Count());
  
                Assert.IsNotNull(djson["collection"]["items"][0]["data"].SingleOrDefault(x => x["name"].Value<string>() == "properties"));
                var propertyCollection = djson["collection"]["items"][0]["data"].Single(x => x["name"].Value<string>() == "properties")["items"];
                Assert.AreEqual(2, propertyCollection.Count());

                Assert.AreEqual("", propertyCollection[0]["data"].Single(x => x["name"].Value<string>() == "regexp")["value"].Value<string>());
                Assert.AreEqual("zyxw", propertyCollection[1]["data"].Single(x => x["name"].Value<string>() == "regexp")["value"].Value<string>());
                Assert.AreEqual("true", propertyCollection[0]["data"].Single(x => x["name"].Value<string>() == "required")["value"].Value<string>());
                Assert.AreEqual("false", propertyCollection[1]["data"].Single(x => x["name"].Value<string>() == "required")["value"].Value<string>());
                
                Assert.AreEqual(2, djson["collection"]["items"][0]["links"].Count());

                Assert.AreEqual("children", djson["collection"]["items"][0]["links"][0]["rel"].Value<string>());
                Assert.AreEqual("parent", djson["collection"]["items"][0]["links"][1]["rel"].Value<string>());
                Assert.AreEqual("Children", djson["collection"]["items"][0]["links"][0]["prompt"].Value<string>());
                Assert.AreEqual("Parent", djson["collection"]["items"][0]["links"][1]["prompt"].Value<string>());
                Assert.AreEqual("http://testserver/umbraco/v1/content/123/children", djson["collection"]["items"][0]["links"][0]["href"].Value<string>());
                Assert.AreEqual("http://testserver/umbraco/v1/content/456", djson["collection"]["items"][0]["links"][1]["href"].Value<string>());

                //TODO: Need to assert more values!
            }
        }

        [Test]
        public async void Get_Empty_Is_200_Response()
        {
            var startup = new TestStartup<IContent>(
                //This will be invoked before the controller is created so we can modify these mocked services,
                // it needs to return the required reader/writer for the tests
                (request, umbCtx, typedContent, contentService, mediaService, memberService) =>
                {
                    var mockContentService = Mock.Get(contentService);
                    mockContentService.Setup(x => x.GetRootContent()).Returns(Enumerable.Empty<IContent>());

                    return new Tuple<ICollectionJsonDocumentWriter<IContent>, ICollectionJsonDocumentReader<IContent>>(
                        new ContentDocumentWriter(request, umbCtx.UrlProvider, mockContentService.Object),
                        null);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.GetAsync(
                    string.Format("http://testserver/umbraco/v1/{0}", RouteConstants.ContentSegment));
                Console.WriteLine(result);
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Test]
        public async void Post_Is_200_Response()
        {
            var startup = new TestStartup<IContent>(
                (request, umbCtx, typedContent, contentService, mediaService, memberService) => new Tuple<ICollectionJsonDocumentWriter<IContent>, ICollectionJsonDocumentReader<IContent>>(
                    new ContentDocumentWriter(request, umbCtx.UrlProvider, Mock.Of<IContentService>()),
                    null));

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.PostAsync(
                    new Uri(string.Format("http://testserver/umbraco/v1/{0}", RouteConstants.ContentSegment)),
                    new CollectionJsonContent(new Collection()));

                Console.WriteLine(result);

                //NOTE: NotImplemented because we cannot post for published content
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Test]
        public async void Put_Is_200_Response()
        {
            var startup = new TestStartup<IContent>(
                (request, umbCtx, typedContent, contentService, mediaService, memberService) => new Tuple<ICollectionJsonDocumentWriter<IContent>, ICollectionJsonDocumentReader<IContent>>(
                    new ContentDocumentWriter(request, umbCtx.UrlProvider, Mock.Of<IContentService>()),
                    null));

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.PutAsync(
                    new Uri(string.Format("http://testserver/umbraco/v1/{0}/123", RouteConstants.ContentSegment)),
                    new CollectionJsonContent(new Collection()));

                Console.WriteLine(result);

                //NOTE: NotImplemented because we cannot post for published content
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Test]
        public async void Delete_Is_200_Response()
        {
            var startup = new TestStartup<IContent>(
                (request, umbCtx, typedContent, contentService, mediaService, memberService) => new Tuple<ICollectionJsonDocumentWriter<IContent>, ICollectionJsonDocumentReader<IContent>>(
                    new ContentDocumentWriter(request, umbCtx.UrlProvider, Mock.Of<IContentService>()),
                    null));

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.DeleteAsync(
                    new Uri(string.Format("http://testserver/umbraco/v1/{0}/123", RouteConstants.ContentSegment)));

                Console.WriteLine(result);

                //NOTE: NotImplemented because we cannot post for published content
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        private static IContent SimpleMockedContent(int id = 123)
        {
            return Mock.Of<IContent>(
                content => content.Id == id
                           && content.Published == true
                           && content.CreateDate == DateTime.Now.AddDays(-2)
                           && content.CreatorId == 0
                           && content.HasIdentity == true
                           && content.ContentType == Mock.Of<IContentType>(ct => ct.Id == 99 && ct.Alias == "testType")
                           && content.ContentTypeId == 10
                           && content.Level == 1
                           && content.Name == "Home"
                           && content.Path == "-1,123"
                           && content.ParentId == 456
                           && content.SortOrder == 1
                           && content.Template == Mock.Of<ITemplate>(te => te.Id == 9 && te.Alias == "home")
                           && content.UpdateDate == DateTime.Now.AddDays(-1)
                           && content.WriterId == 1
                           && content.PropertyTypes == new List<PropertyType>(new[]
                           {
                               new PropertyType("testEditor", DataTypeDatabaseType.Nvarchar, "testProperty1") {Name = "Test Property1", Mandatory = true, ValidationRegExp = ""},
                               new PropertyType("testEditor", DataTypeDatabaseType.Nvarchar, "testProperty2") {Name = "Test Property2", Mandatory = false, ValidationRegExp = "zyxw"}
                           })
                           && content.Properties == new PropertyCollection(new[]
                           {
                               new Property(3, Guid.NewGuid(),
                                   new PropertyType("testEditor", DataTypeDatabaseType.Nvarchar, "testProperty1"), "property value1"),
                               new Property(3, Guid.NewGuid(),
                                   new PropertyType("testEditor", DataTypeDatabaseType.Nvarchar, "testProperty2"), "property value2"),
                           }));
        }

    }
}