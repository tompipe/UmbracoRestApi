using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Examine;
using Examine.LuceneEngine;
using Examine.Providers;
using Examine.SearchCriteria;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.RestApi.Routing;
using Umbraco.RestApi.Tests.TestHelpers;
using System.IO;

namespace Umbraco.RestApi.Tests
{
    [TestFixture]
    public class MembersControllerTests
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
            typeof(PropertyEditorResolver).CallStaticMethod("Reset", true);
        }


        [Test]
        public async void Get_Root_With_OPTIONS()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services,
                (request, umbCtx, typedContent, serviceContext, searchProvider) =>
                {
                    var mockMemberService = Mock.Get(serviceContext.MemberService);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}", RouteConstants.MembersSegment)),
                    Method = HttpMethod.Options,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                request.Headers.Add("Access-Control-Request-Headers", "accept, authorization");
                request.Headers.Add("Access-Control-Request-Method", "GET");
                request.Headers.Add("Origin", "http://localhost:12061");
                request.Headers.Add("Referer", "http://localhost:12061/browser.html");

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
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
                    var mockMemberService = Mock.Get(serviceContext.MemberService);
                    var mockedOut = 0;
                    mockMemberService.Setup(x => x.GetAll(It.IsAny<int>(), 100, out mockedOut)).Returns(new[]
                    {
                        ModelMocks.SimpleMockedMember(123, -1),
                        ModelMocks.SimpleMockedMember(456, -1)
                    });
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}", RouteConstants.MembersSegment)),
                    Method = HttpMethod.Get,
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

                var asdf = GlobalConfiguration.Configuration;

                var djson = JsonConvert.DeserializeObject<JObject>(json);

                Assert.AreEqual("/umbraco/rest/v1/members{?pageIndex,pageSize,orderBy,direction,memberTypeAlias,filter}", djson["_links"]["root"]["href"].Value<string>());
                Assert.AreEqual(0, djson["totalResults"].Value<int>());
              
            }
        }

        [Test]
        public async void Search_200_Result()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, serviceContext, searchProvider) =>
                {
                    var mockSearchResults = new Mock<ISearchResults>();
                    mockSearchResults.Setup(results => results.TotalItemCount).Returns(10);
                    mockSearchResults.Setup(results => results.Skip(It.IsAny<int>())).Returns(new[]
                    {
                        new SearchResult() {Id = 789},
                        new SearchResult() {Id = 456},
                    });

                    var mockSearchProvider = Mock.Get(searchProvider);
                    mockSearchProvider.Setup(x => x.CreateSearchCriteria()).Returns(Mock.Of<ISearchCriteria>());
                    mockSearchProvider.Setup(x => x.Search(It.IsAny<ISearchCriteria>(), It.IsAny<int>()))
                        .Returns(mockSearchResults.Object);

                    var mockMemberService = Mock.Get(serviceContext.MemberService);
                    mockMemberService.Setup(x => x.GetAllMembers( It.IsAny<int[]>()))
                        .Returns(new[]
                        {
                            ModelMocks.SimpleMockedMember(789),
                            ModelMocks.SimpleMockedMember(456)
                        });
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/search?lucene=parentID:\\-1", RouteConstants.MembersSegment)),
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
                     var mockMemberService = Mock.Get(serviceContext.MemberService);
                     mockMemberService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedMember());
                 });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/123", RouteConstants.MembersSegment)),
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

                Assert.AreEqual("/umbraco/rest/v1/members/123", djson["_links"]["self"]["href"].Value<string>());
                Assert.AreEqual("/umbraco/rest/v1/members{?pageIndex,pageSize,orderBy,direction,memberTypeAlias,filter}", djson["_links"]["root"]["href"].Value<string>());

                var properties = djson["properties"].ToObject<IDictionary<string, object>>();
                Assert.AreEqual(2, properties.Count());
                Assert.IsTrue(properties.ContainsKey("TestProperty1"));
                Assert.IsTrue(properties.ContainsKey("testProperty2"));
            }
        }

        [Test]
        public async void Get_Metadata_Result()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                 (request, umbCtx, typedContent, serviceContext, searchProvider) =>
                 {
                     var mockMemberService = Mock.Get(serviceContext.MemberService);

                     mockMemberService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedMember());
                     var mockTextService = Mock.Get(serviceContext.TextService);

                     mockTextService.Setup(x => x.Localize(It.IsAny<string>(), It.IsAny<CultureInfo>(), It.IsAny<IDictionary<string, string>>()))
                         .Returns((string input, CultureInfo culture, IDictionary<string, string> tokens) => input);
                 });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/123/meta", RouteConstants.MembersSegment)),
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
        public async void Post_Is_201_Response()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, serviceContext, searchProvider) =>
                {
                   MemberServiceMocks.SetupMocksForPost(serviceContext);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}", RouteConstants.MembersSegment)),
                    Method = HttpMethod.Post,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                request.Content = new StringContent(@"{
  ""memberTypeAlias"": ""testType"",
  ""name"": ""John Johnson"",
  ""email"" : ""john@johnson.com"",
  ""loginName"" : ""johnjohnson"",
  ""properties"": {
    ""TestProperty1"": ""property value1"",
    ""testProperty2"": ""property value2""
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
        public async void Post_Is_400_Validation_Required_Fields()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, serviceContext, searchProvider) =>
                {
                    MemberServiceMocks.SetupMocksForPost(serviceContext);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}", RouteConstants.MembersSegment)),
                    Method = HttpMethod.Post,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                //NOTE: it is missing
                request.Content = new StringContent(@"{
  ""memberTypeAlias"": """",
  ""name"": ""John Johnson"",
  ""email"" : """",
  ""loginName"" : ""johnjohnson"",
  ""properties"": {
    ""TestProperty1"": ""property value1"",
    ""testProperty2"": ""property value2""
  }
}", Encoding.UTF8, "application/json");

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

                var djson = JsonConvert.DeserializeObject<JObject>(json);

                Assert.AreEqual(2, djson["totalResults"].Value<int>());
                Assert.AreEqual("content.memberTypeAlias", djson["_embedded"]["errors"][0]["logRef"].Value<string>());
                Assert.AreEqual("content.email", djson["_embedded"]["errors"][1]["logRef"].Value<string>());
            }
        }

        [Test]
        public async void Post_Is_400_Validation_Property_Missing()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, serviceContext, searchProvider) =>
                {
                    MemberServiceMocks.SetupMocksForPost(serviceContext);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}", RouteConstants.MembersSegment)),
                    Method = HttpMethod.Post,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                //NOTE: it is missing
                request.Content = new StringContent(@"{
  ""memberTypeAlias"": ""testType"",
  ""name"": ""John Johnson"",
  ""email"" : ""john@johnson.com"",
  ""loginName"" : ""johnjohnson"",
  ""parentId"": 456,
  ""templateId"": 9,
  ""properties"": {
    ""thisDoesntExist"": ""property value1"",
    ""testProperty2"": ""property value2""
  }
}", Encoding.UTF8, "application/json");

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

                var djson = JsonConvert.DeserializeObject<JObject>(json);

                Assert.AreEqual(1, djson["totalResults"].Value<int>());
                Assert.AreEqual("content.properties.thisDoesntExist", djson["_embedded"]["errors"][0]["logRef"].Value<string>());

            }
        }

        [Test]
        public async void Post_Is_400_Validation_Property_Required()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, serviceContext, searchProvider) =>
                {
                    MemberServiceMocks.SetupMocksForPost(serviceContext);

                    var mockPropertyEditor = Mock.Get(PropertyEditorResolver.Current);
                    mockPropertyEditor.Setup(x => x.GetByAlias("testEditor")).Returns(new ModelMocks.SimplePropertyEditor());
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}", RouteConstants.MembersSegment)),
                    Method = HttpMethod.Post,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                //NOTE: it is missing
                request.Content = new StringContent(@"{
   ""memberTypeAlias"": ""testType"",
  ""name"": ""John Johnson"",
  ""email"" : ""john@johnson.com"",
  ""loginName"" : ""johnjohnson"",
  ""properties"": {
    ""TestProperty1"": """",
    ""testProperty2"": ""property value2""
  }
}", Encoding.UTF8, "application/json");

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

                var djson = JsonConvert.DeserializeObject<JObject>(json);

                Assert.AreEqual(1, djson["totalResults"].Value<int>());
                Assert.AreEqual("content.properties.TestProperty1.value", djson["_embedded"]["errors"][0]["logRef"].Value<string>());

            }
        }

        [Test]
        public async void Put_Is_200_Response()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, serviceContext, searchProvider) =>
                {
                    MemberServiceMocks.SetupMocksForPost(serviceContext);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/123", RouteConstants.MembersSegment)),
                    Method = HttpMethod.Put,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                request.Content = new StringContent(@"{
  ""memberTypeAlias"": ""testType"",
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

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }


        [Test]
        public async void Put_Upload_Is_200_Response()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (request, umbCtx, typedContent, serviceContext, searchProvider) =>
                {
                    MemberServiceMocks.SetupMocksForPost(serviceContext);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                MultipartFormDataContent mfdc = new MultipartFormDataContent();
                mfdc.Add(
                        new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("This is from a file"))),
                        name: "Data",
                        fileName: "File1.txt");

                var uri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}/123/upload", RouteConstants.MembersSegment));
                var result = await server.HttpClient.PutAsync(uri, mfdc);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }



        
    }

}