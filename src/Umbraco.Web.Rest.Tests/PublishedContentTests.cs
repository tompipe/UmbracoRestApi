using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using Microsoft.Owin.Testing;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Security.Providers;

namespace Umbraco.Web.Rest.Tests
{
    [TestFixture]
    public class PublishedContentTests
    {
        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        public async void DoTest()
        {
            using (var server = TestServer.Create<TestStartup>())
            {
                ConfigurationManager.AppSettings.Set("umbracoPath", "~/umbraco");
                
                var userMembershipProvider = new UsersMembershipProvider(Mock.Of<IMembershipMemberService<IUser>>());
                userMembershipProvider.Initialize("UsersMembershipProvider", new NameValueCollection());
                
                // Execute test against the web API.
                var result = await server.HttpClient.GetAsync("http://testserver/umbraco/v1/content/123");

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

    }
}

