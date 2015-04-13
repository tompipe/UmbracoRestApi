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
using Microsoft.Owin.Testing;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Rest.Tests.TestHelpers;
using Umbraco.Web.Security.Providers;

namespace Umbraco.Web.Rest.Tests
{
    [TestFixture]
    public class PublishedContentTests
    {
        [TestFixtureSetUp]
        public void TearDown()
        {
            ConfigurationManager.AppSettings.Set("umbracoPath", "~/umbraco");
            ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", "7.3.0");
        }

        [Test]
        public async void Get_Id_Is_200_Response()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (typedContent, contentService, mediaService, memberService) =>
                {
                    var mockTypedContent = Mock.Get(typedContent);
                    mockTypedContent.Setup(x => x.TypedContent(It.IsAny<int>())).Returns(SimpleMockedPublishedContent);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.GetAsync("http://testserver/umbraco/v1/content/123");
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Test]
        public async void Get_Empty_Is_200_Response()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (typedContent, contentService, mediaService, memberService) =>
                {
                    var mockTypedContent = Mock.Get(typedContent);
                    mockTypedContent.Setup(x => x.TypedContentAtRoot()).Returns(Enumerable.Empty<IPublishedContent>());
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var result = await server.HttpClient.GetAsync("http://testserver/umbraco/v1/content");
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
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

