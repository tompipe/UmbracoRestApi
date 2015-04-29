using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Umbraco.Web.Rest.Models;
using Umbraco.Web.Rest.Serialization;
using WebApi.Hal;

namespace Umbraco.Web.Rest.Tests
{
    [TestFixture]
    public class CamelCaseTests
    {
        [Test]
        public void Property_Aliases_Not_Changed()
        {
            var obj = new ContentRepresentation()
            {
                Properties = new Dictionary<string, ContentPropertyRepresentation>
                {
                    {"Property1", new ContentPropertyRepresentation() {Value = "value 1", Label = "Hello"}},
                    {"property 2", new ContentPropertyRepresentation() {Value = "value 2", Label = "World"}}
                }
            };
            
            var serialize = JsonConvert.SerializeObject(obj);

            var deserialize = JsonConvert.DeserializeObject<ContentRepresentation>(serialize);

            Assert.IsTrue(deserialize.Properties.Keys.Contains("Property1"));
            Assert.IsTrue(deserialize.Properties.Keys.Contains("property 2"));
        }
    }
}