using System;
using System.Configuration;
using System.Linq;
using System.Net;
using CollectionJson;
using CollectionJson.Client;
using Microsoft.Owin.Testing;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Web.Rest.Routing;
using Umbraco.Web.Rest.Serialization;
using Umbraco.Web.Rest.Tests.TestHelpers;

namespace Umbraco.Web.Rest.Tests
{
    [TestFixture]
    public class ContentControllerRoutingTests
    {
        [TestFixtureSetUp]
        public void TearDown()
        {
            ConfigurationManager.AppSettings.Set("umbracoPath", "~/umbraco");
            ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", UmbracoVersion.Current.ToString(3));
        }

        [Test]
        public async void Get_Id_Is_200_Response()
        {
            var startup = new TestStartup<IContent>(
                //This will be invoked before the controller is created so we can modify these mocked services
                // it needs to return the required reader/writer for the tests
                (request, typedContent, contentService, mediaService, memberService) =>
                {
                    var mockContentService = Mock.Get(contentService);
                    mockContentService.Setup(x => x.GetById(It.IsAny<int>())).Returns(SimpleMockedContent);

                    return new Tuple<ICollectionJsonDocumentWriter<IContent>, ICollectionJsonDocumentReader<IContent>>(
                        new ContentDocumentWriter(request),
                        null);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.GetAsync(
                    string.Format("http://testserver/umbraco/v1/{0}/123", RouteConstants.ContentSegment));
                Console.WriteLine(result);
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Test]
        public async void Get_Empty_Is_200_Response()
        {
            var startup = new TestStartup<IContent>(
                //This will be invoked before the controller is created so we can modify these mocked services,
                // it needs to return the required reader/writer for the tests
                (request, typedContent, contentService, mediaService, memberService) =>
                {
                    var mockContentService = Mock.Get(contentService);
                    mockContentService.Setup(x => x.GetRootContent()).Returns(Enumerable.Empty<IContent>());

                    return new Tuple<ICollectionJsonDocumentWriter<IContent>, ICollectionJsonDocumentReader<IContent>>(
                        new ContentDocumentWriter(request),
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
                (request, typedContent, contentService, mediaService, memberService) => new Tuple<ICollectionJsonDocumentWriter<IContent>, ICollectionJsonDocumentReader<IContent>>(
                    new ContentDocumentWriter(request),
                    null));

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.PostAsync(
                    new Uri(string.Format("http://testserver/umbraco/v1/{0}", RouteConstants.ContentSegment)),
                    new CollectionJsonContent(new Collection()));

                Console.WriteLine(result);

                //NOTE: NotImplemented because we cannot post for published content
                Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
            }
        }

        [Test]
        public async void Put_Is_200_Response()
        {
            var startup = new TestStartup<IContent>(
                (request, typedContent, contentService, mediaService, memberService) => new Tuple<ICollectionJsonDocumentWriter<IContent>, ICollectionJsonDocumentReader<IContent>>(
                    new ContentDocumentWriter(request),
                    null));

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.PutAsync(
                    new Uri(string.Format("http://testserver/umbraco/v1/{0}/123", RouteConstants.ContentSegment)),
                    new CollectionJsonContent(new Collection()));

                Console.WriteLine(result);

                //NOTE: NotImplemented because we cannot post for published content
                Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
            }
        }

        [Test]
        public async void Delete_Is_200_Response()
        {
            var startup = new TestStartup<IContent>(
                (request, typedContent, contentService, mediaService, memberService) => new Tuple<ICollectionJsonDocumentWriter<IContent>, ICollectionJsonDocumentReader<IContent>>(
                    new ContentDocumentWriter(request),
                    null));

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.DeleteAsync(
                    new Uri(string.Format("http://testserver/umbraco/v1/{0}/123", RouteConstants.ContentSegment)));

                Console.WriteLine(result);

                //NOTE: NotImplemented because we cannot post for published content
                Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
            }
        }

        private static IContent SimpleMockedContent()
        {
            return Mock.Of<IContent>(content => content.Id == 123
                                                         && content.Published == true
                                                         && content.CreateDate == DateTime.Now.AddDays(-2)
                                                         && content.CreatorId == 0
                                                         && content.HasIdentity == true
                                                         && content.ContentType == Mock.Of<IContentType>(ct => ct.Id == 99 && ct.Alias == "testType")
                                                         && content.ContentTypeId == 10                                                         
                                                         && content.Level == 1
                                                         && content.Name == "Home"
                                                         && content.Path == "-1,123"
                                                         && content.SortOrder == 1
                                                         && content.Template == Mock.Of<ITemplate>(te => te.Id == 9 && te.Alias == "home")
                                                         && content.UpdateDate == DateTime.Now.AddDays(-1)                                                         
                                                         && content.WriterId == 1);
        }

    }
}