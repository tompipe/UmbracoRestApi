using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Security;
using CollectionJson;
using CollectionJson.Client;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Rest.Routing;
using Umbraco.Web.Rest.Serialization;
using Umbraco.Web.Rest.Tests.TestHelpers;
using Umbraco.Web.Security.Providers;

namespace Umbraco.Web.Rest.Tests
{
    [TestFixture]
    public class CollectionJsonPublishedContentControllerTests
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
            var startup = new CollectionJsonTestStartup<IPublishedContent>(
                //This will be invoked before the controller is created so we can modify these mocked services
                // it needs to return the required reader/writer for the tests
                (request, umbCtx, typedContent, contentService, mediaService, memberService) =>
                {
                    var mockTypedContent = Mock.Get(typedContent);
                    mockTypedContent.Setup(x => x.TypedContent(It.IsAny<int>())).Returns(() => SimpleMockedPublishedContent(123, 456, 789));

                    return new Tuple<ICollectionJsonDocumentWriter<IPublishedContent>, ICollectionJsonDocumentReader<IPublishedContent>>(
                        new PublishedContentDocumentWriter(request, umbCtx.UrlProvider),
                        null);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.GetAsync(
                    string.Format("http://testserver/umbraco/rest/v1/{0}/{1}/123/children", RouteConstants.ContentSegment, RouteConstants.PublishedSegment));
                Console.WriteLine(result);
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Test]
        public async void Get_Id_Result()
        {
            var startup = new CollectionJsonTestStartup<IPublishedContent>(
                //This will be invoked before the controller is created so we can modify these mocked services
                // it needs to return the required reader/writer for the tests
                (request, umbCtx, typedContent, contentService, mediaService, memberService) =>
                {
                    var mockTypedContent = Mock.Get(typedContent);
                    mockTypedContent.Setup(x => x.TypedContent(It.IsAny<int>())).Returns(() => SimpleMockedPublishedContent(123, 456, 789));

                    return new Tuple<ICollectionJsonDocumentWriter<IPublishedContent>, ICollectionJsonDocumentReader<IPublishedContent>>(
                        new PublishedContentDocumentWriter(request, umbCtx.UrlProvider),
                        null);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.GetAsync(
                    string.Format("http://testserver/umbraco/rest/v1/{0}/{1}/123", RouteConstants.ContentSegment, RouteConstants.PublishedSegment));
                Console.WriteLine(result);

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
                Assert.AreEqual("application/vnd.collection+json", result.Content.Headers.ContentType.MediaType);
                Assert.IsAssignableFrom<StreamContent>(result.Content);
                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Assert.IsTrue(json.Contains("\"collection\""));

                var djson = JsonConvert.DeserializeObject<JObject>(json);
                Assert.AreEqual("http://testserver/umbraco/rest/v1/content/published", djson["collection"]["href"].Value<string>());
                Assert.AreEqual(1, djson["collection"]["items"].Count());
                Assert.AreEqual("http://testserver/umbraco/rest/v1/content/published/123", djson["collection"]["items"][0]["href"].Value<string>());
                Assert.AreEqual(12, djson["collection"]["items"][0]["data"].Count());

                Assert.IsNotNull(djson["collection"]["items"][0]["data"].SingleOrDefault(x => x[FieldNames.Name].Value<string>() == FieldNames.Properties));
                Assert.AreEqual(2, djson["collection"]["items"][0]["data"].SingleOrDefault(x => x[FieldNames.Name].Value<string>() == FieldNames.Properties)["array"].Count());

                Assert.AreEqual(2, djson["collection"]["items"][0]["links"].Count());

                Assert.AreEqual("children", djson["collection"]["items"][0]["links"][0]["rel"].Value<string>());
                Assert.AreEqual("parent", djson["collection"]["items"][0]["links"][1]["rel"].Value<string>());
                Assert.AreEqual("Children", djson["collection"]["items"][0]["links"][0]["prompt"].Value<string>());
                Assert.AreEqual("Parent", djson["collection"]["items"][0]["links"][1]["prompt"].Value<string>());
                Assert.AreEqual("http://testserver/umbraco/rest/v1/content/published/123/children", djson["collection"]["items"][0]["links"][0]["href"].Value<string>());
                Assert.AreEqual("http://testserver/umbraco/rest/v1/content/published/456", djson["collection"]["items"][0]["links"][1]["href"].Value<string>());

                //TODO: Need to assert more values!
            }
        }

        [Test]
        public async void Get_Empty_Is_200_Response()
        {
            var startup = new CollectionJsonTestStartup<IPublishedContent>(
                //This will be invoked before the controller is created so we can modify these mocked services,
                // it needs to return the required reader/writer for the tests
                (request, umbCtx, typedContent, contentService, mediaService, memberService) =>
                {
                    var mockTypedContent = Mock.Get(typedContent);
                    mockTypedContent.Setup(x => x.TypedContentAtRoot()).Returns(Enumerable.Empty<IPublishedContent>());

                    return new Tuple<ICollectionJsonDocumentWriter<IPublishedContent>, ICollectionJsonDocumentReader<IPublishedContent>>(
                        new PublishedContentDocumentWriter(request, umbCtx.UrlProvider),
                        null);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.GetAsync(
                    string.Format("http://testserver/umbraco/rest/v1/{0}/{1}", RouteConstants.ContentSegment, RouteConstants.PublishedSegment));
                Console.WriteLine(result);
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Test]
        public async void Post_Is_501_Response()
        {
            var startup = new CollectionJsonTestStartup<IPublishedContent>(
                (request, umbCtx, typedContent, contentService, mediaService, memberService) => new Tuple<ICollectionJsonDocumentWriter<IPublishedContent>, ICollectionJsonDocumentReader<IPublishedContent>>(
                    new PublishedContentDocumentWriter(request, umbCtx.UrlProvider),
                    null));

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.PostAsync(
                    new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/{1}", RouteConstants.ContentSegment, RouteConstants.PublishedSegment)),
                    new CollectionJsonContent(new Collection()));

                Console.WriteLine(result);

                //NOTE: NotImplemented because we cannot post for published content
                Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
            }
        }

        [Test]
        public async void Put_Is_501_Response()
        {
            var startup = new CollectionJsonTestStartup<IPublishedContent>(
                (request, umbCtx, typedContent, contentService, mediaService, memberService) => new Tuple<ICollectionJsonDocumentWriter<IPublishedContent>, ICollectionJsonDocumentReader<IPublishedContent>>(
                    new PublishedContentDocumentWriter(request, umbCtx.UrlProvider),
                    null));

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.PutAsync(
                    new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/{1}/123", RouteConstants.ContentSegment, RouteConstants.PublishedSegment)),
                    new CollectionJsonContent(new Collection()));

                Console.WriteLine(result);

                //NOTE: NotImplemented because we cannot post for published content
                Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
            }
        }

        [Test]
        public async void Delete_Is_501_Response()
        {
            var startup = new CollectionJsonTestStartup<IPublishedContent>(
                (request, umbCtx, typedContent, contentService, mediaService, memberService) => new Tuple<ICollectionJsonDocumentWriter<IPublishedContent>, ICollectionJsonDocumentReader<IPublishedContent>>(
                    new PublishedContentDocumentWriter(request, umbCtx.UrlProvider),
                    null));

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.DeleteAsync(
                    new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/{1}/123", RouteConstants.ContentSegment, RouteConstants.PublishedSegment)));

                Console.WriteLine(result);

                //NOTE: NotImplemented because we cannot post for published content
                Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
            }
        }

        private static IPublishedContent SimpleMockedPublishedContent(int id = 123, int? parentId = null, int? childId = null)
        {
            return Mock.Of<IPublishedContent>(
                content => content.Id == id
                           && content.IsDraft == false
                           && content.CreateDate == DateTime.Now.AddDays(-2)
                           && content.CreatorId == 0
                           && content.CreatorName == "admin"
                           && content.DocumentTypeAlias == "test"
                           && content.DocumentTypeId == 10
                           && content.ItemType == PublishedItemType.Content
                           && content.Level == 1
                           && content.Name == "Home"
                           && content.Path == "-1,123"
                           && content.SortOrder == 1
                           && content.TemplateId == 9
                           && content.UpdateDate == DateTime.Now.AddDays(-1)
                           && content.Url == "/home"
                           && content.UrlName == "home"
                           && content.WriterId == 1
                           && content.WriterName == "Editor"
                           && content.Properties == new List<IPublishedProperty>(new[]
                           {
                               Mock.Of<IPublishedProperty>(property => property.HasValue == true
                                                                       && property.PropertyTypeAlias == "testProperty1"
                                                                       && property.DataValue == "raw value"
                                                                       && property.Value == "Property Value"),
                               Mock.Of<IPublishedProperty>(property => property.HasValue == true
                                                                       && property.PropertyTypeAlias == "testProperty2"
                                                                       && property.DataValue == "raw value"
                                                                       && property.Value == "Property Value")
                           })
                           && content.Parent == (parentId.HasValue ? SimpleMockedPublishedContent(parentId.Value, null, null) : null)
                           && content.Children == (childId.HasValue ? new[] {SimpleMockedPublishedContent(childId.Value, null, null)} : Enumerable.Empty<IPublishedContent>()));
        }

    }
}

