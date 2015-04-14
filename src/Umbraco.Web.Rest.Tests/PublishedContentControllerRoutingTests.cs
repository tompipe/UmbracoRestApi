using System;
using System.Collections.Specialized;
using System.Configuration;
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
    public class PublishedContentControllerRoutingTests
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
            var startup = new TestStartup<IPublishedContent>(
                //This will be invoked before the controller is created so we can modify these mocked services
                // it needs to return the required reader/writer for the tests
                (request, typedContent, contentService, mediaService, memberService) =>
                {
                    var mockTypedContent = Mock.Get(typedContent);
                    mockTypedContent.Setup(x => x.TypedContent(It.IsAny<int>())).Returns(SimpleMockedPublishedContent);

                    return new Tuple<ICollectionJsonDocumentWriter<IPublishedContent>, ICollectionJsonDocumentReader<IPublishedContent>>(
                        new PublishedContentDocumentWriter(request),
                        null);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.GetAsync(
                    string.Format("http://testserver/umbraco/v1/{0}/{1}/123", RouteConstants.ContentSegment, RouteConstants.PublishedSegment));
                Console.WriteLine(result);
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Test]
        public async void Get_Empty_Is_200_Response()
        {
            var startup = new TestStartup<IPublishedContent>(
                //This will be invoked before the controller is created so we can modify these mocked services,
                // it needs to return the required reader/writer for the tests
                (request, typedContent, contentService, mediaService, memberService) =>
                {
                    var mockTypedContent = Mock.Get(typedContent);
                    mockTypedContent.Setup(x => x.TypedContentAtRoot()).Returns(Enumerable.Empty<IPublishedContent>());

                    return new Tuple<ICollectionJsonDocumentWriter<IPublishedContent>, ICollectionJsonDocumentReader<IPublishedContent>>(
                        new PublishedContentDocumentWriter(request),
                        null);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.GetAsync(
                    string.Format("http://testserver/umbraco/v1/{0}/{1}", RouteConstants.ContentSegment, RouteConstants.PublishedSegment));
                Console.WriteLine(result);
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Test]
        public async void Post_Is_501_Response()
        {
            var startup = new TestStartup<IPublishedContent>(
                (request, typedContent, contentService, mediaService, memberService) => new Tuple<ICollectionJsonDocumentWriter<IPublishedContent>, ICollectionJsonDocumentReader<IPublishedContent>>(
                    new PublishedContentDocumentWriter(request),
                    null));

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.PostAsync(
                    new Uri(string.Format("http://testserver/umbraco/v1/{0}/{1}", RouteConstants.ContentSegment, RouteConstants.PublishedSegment)),
                    new CollectionJsonContent(new Collection()));

                Console.WriteLine(result);

                //NOTE: NotImplemented because we cannot post for published content
                Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
            }
        }

        [Test]
        public async void Put_Is_501_Response()
        {
            var startup = new TestStartup<IPublishedContent>(
                (request, typedContent, contentService, mediaService, memberService) => new Tuple<ICollectionJsonDocumentWriter<IPublishedContent>, ICollectionJsonDocumentReader<IPublishedContent>>(
                    new PublishedContentDocumentWriter(request),
                    null));

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.PutAsync(
                    new Uri(string.Format("http://testserver/umbraco/v1/{0}/{1}/123", RouteConstants.ContentSegment, RouteConstants.PublishedSegment)),
                    new CollectionJsonContent(new Collection()));

                Console.WriteLine(result);

                //NOTE: NotImplemented because we cannot post for published content
                Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
            }
        }

        [Test]
        public async void Delete_Is_501_Response()
        {
            var startup = new TestStartup<IPublishedContent>(
                (request, typedContent, contentService, mediaService, memberService) => new Tuple<ICollectionJsonDocumentWriter<IPublishedContent>, ICollectionJsonDocumentReader<IPublishedContent>>(
                    new PublishedContentDocumentWriter(request),
                    null));

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.DeleteAsync(
                    new Uri(string.Format("http://testserver/umbraco/v1/{0}/{1}/123", RouteConstants.ContentSegment, RouteConstants.PublishedSegment)));

                Console.WriteLine(result);

                //NOTE: NotImplemented because we cannot post for published content
                Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
            }
        }

        private static IPublishedContent SimpleMockedPublishedContent()
        {
            return Mock.Of<IPublishedContent>(content => content.Id == 123
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
                                                         && content.WriterName == "Editor");
        }

    }
}

