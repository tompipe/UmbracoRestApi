using System;
using System.Web.Http.Controllers;
using WebApi.Hal;

namespace Umbraco.Web.Rest.Controllers.HAL
{
    public class HalFormatterConfigurationAttribute : Attribute, IControllerConfiguration
    {
        public virtual void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Formatters.Insert(0, new XmlHalMediaTypeFormatter());
            controllerSettings.Formatters.Insert(0, new JsonHalMediaTypeFormatter());
        }
    }
}