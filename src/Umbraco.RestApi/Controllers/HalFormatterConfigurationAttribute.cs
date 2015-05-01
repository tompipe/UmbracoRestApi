using System;
using System.Web.Http.Controllers;
using Newtonsoft.Json.Serialization;
using WebApi.Hal;

namespace Umbraco.RestApi.Controllers
{
    public class HalFormatterConfigurationAttribute : Attribute, IControllerConfiguration
    {
        public virtual void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {

            

            controllerSettings.Formatters.Insert(0, new XmlHalMediaTypeFormatter());
            var jsonFormatter = new JsonHalMediaTypeFormatter
            {
                SerializerSettings =
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            };
            controllerSettings.Formatters.Insert(0, jsonFormatter);
        }
    }
}