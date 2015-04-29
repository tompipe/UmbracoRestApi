using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using AutoMapper;
using CollectionJson;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Web.Rest.Models;
using Umbraco.Web.Rest.Routing;
using Umbraco.Web.Rest.Serialization;
using Umbraco.Web.Rest.Tests.TestHelpers;

namespace Umbraco.Web.Rest.Tests
{
    [TestFixture]
    public class ODataContentControllerTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            ConfigurationManager.AppSettings.Set("umbracoPath", "~/umbraco");
            ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", UmbracoVersion.Current.ToString(3));
            var mockSettings = MockUmbracoSettings.GenerateMockSettings();
            UmbracoConfig.For.CallMethod("SetUmbracoSettings", mockSettings);

        }

        //[Test]
        //public async void Root()
        //{
        //    var startup = new ODataTestStartup(
        //        //This will be invoked before the controller is created so we can modify these mocked services
        //        (request, umbCtx, typedContent, contentService, mediaService, memberService) =>
        //        {
        //            var mockContentService = Mock.Get(contentService);

        //            mockContentService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedContent());

        //            mockContentService.Setup(x => x.GetChildren(It.IsAny<int>())).Returns(new List<IContent>(new[] { ModelMocks.SimpleMockedContent(789) }));
        //        });

        //    using (var server = TestServer.Create(builder => startup.Configuration(builder)))
        //    {
        //        var request = new HttpRequestMessage()
        //        {
        //            RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/odata")),
        //            Method = HttpMethod.Get,
        //        };
        //        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        Console.WriteLine(request);
        //        var result = await server.HttpClient.SendAsync(request);
        //        Console.WriteLine(result);
        //        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        //    }
        //}

        [Test]
        public async void Post_Is_201_Response()
        {
            var startup = new ODataTestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                 (request, umbCtx, typedContent, contentService, mediaService, memberService) =>
                 {
                     var mockContentService = Mock.Get(contentService);

                     mockContentService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedContent());

                     mockContentService.Setup(x => x.GetChildren(It.IsAny<int>())).Returns(new List<IContent>(new[] { ModelMocks.SimpleMockedContent(789) }));

                     mockContentService.Setup(x => x.CreateContent(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                         .Returns(ModelMocks.SimpleMockedContent(789));
                 });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/odata/{0}", RouteConstants.ContentSegment)),
                    Method = HttpMethod.Post,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(@"{
  ""@odata.context"": ""http://testserver/umbraco/rest/v1/odata/$metadata#Content/$entity"", 
  ""contentTypeAlias"": ""testType"",
  ""parentId"": 456,
  ""templateId"": 9,
  ""name"": ""Home"",
  ""properties"": {
    ""testProperty1"": {
      ""@odata.type"": ""#Umbraco.Web.Rest.Models.ContentItemProperty"",
      ""label"": ""Test Property1"",
      ""value"": ""property value1""
    },
    ""testProperty2"": {
      ""@odata.type"": ""#Umbraco.Web.Rest.Models.ContentItemProperty"",
      ""label"": ""Test Property2"",
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
        public async void Get_Id_Result()
        {
           var startup = new ODataTestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, contentService, mediaService, memberService) =>
                {
                    var mockContentService = Mock.Get(contentService);

                    mockContentService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedContent());

                    mockContentService.Setup(x => x.GetChildren(It.IsAny<int>())).Returns(new List<IContent>(new[] { ModelMocks.SimpleMockedContent(789) }));
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/odata/{0}(123)", RouteConstants.ContentSegment)),
                    Method = HttpMethod.Get,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);
                
                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Test]
        public async void Get_Empty_Is_200_Response()
        {
            var startup = new ODataTestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, contentService, mediaService, memberService) =>
                {
                    var mockContentService = Mock.Get(contentService);
                    mockContentService.Setup(x => x.GetRootContent()).Returns(Enumerable.Empty<IContent>());                    
                });

            
            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/odata/{0}", RouteConstants.ContentSegment)),
                    Method = HttpMethod.Get,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Test]
        public async void Get_Get_Id_MetaData_Result()
        {
            var startup = new ODataTestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                 (request, umbCtx, typedContent, contentService, mediaService, memberService) =>
                 {
                     var mockContentService = Mock.Get(contentService);

                     mockContentService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedContent());

                     mockContentService.Setup(x => x.GetChildren(It.IsAny<int>())).Returns(new List<IContent>(new[] { ModelMocks.SimpleMockedContent(789) }));
                 });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/odata/{0}(123)?$format=application/json;odata.metadata=full", RouteConstants.ContentSegment)),
                    Method = HttpMethod.Get,
                };
                
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //TODO: has no affect
                request.Headers.Add("Prefer", "odata.include-annotations=\" * \"");

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