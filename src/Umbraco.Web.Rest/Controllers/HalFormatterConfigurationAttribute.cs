using System;
using System.Web.Http.Controllers;
using Newtonsoft.Json.Serialization;
using WebApi.Hal;

namespace Umbraco.Web.Rest.Controllers
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
                    //TODO: We don't want camel case the property aliases!! So we'll have to modify this a little
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            };
            controllerSettings.Formatters.Insert(0, jsonFormatter);
        }
    }
}