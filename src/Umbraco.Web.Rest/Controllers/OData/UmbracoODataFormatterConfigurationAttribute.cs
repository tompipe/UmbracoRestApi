using System;
using System.Web.Http.Controllers;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Deserialization;
using Umbraco.Web.Rest.Serialization.OData;

namespace Umbraco.Web.Rest.Controllers.OData
{
    public class UmbracoODataFormatterConfigurationAttribute : Attribute, IControllerConfiguration
    {
        public virtual void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            // create the formatters with the custom serializer provider and use them in the configuration.
            var odataFormatters = ODataMediaTypeFormatters.Create(new CustomODataSerializerProvider(), new DefaultODataDeserializerProvider());
            controllerSettings.Formatters.InsertRange(0, odataFormatters);
        }
    }
}