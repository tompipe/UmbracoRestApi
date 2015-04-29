using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.Formatter.Serialization;
using Microsoft.OData.Edm;

namespace Umbraco.Web.Rest.Serialization.OData
{
    /// <summary>
    /// Custom serializer to invoke the UmbracoAnnotationsEntitySerializer to add custom annotations
    /// </summary>
    public class CustomODataSerializerProvider : DefaultODataSerializerProvider
    {
        private readonly UmbracoAnnotationsEntitySerializer _umbracoAnnotationsEntitySerializer;

        public CustomODataSerializerProvider()
        {
            _umbracoAnnotationsEntitySerializer = new UmbracoAnnotationsEntitySerializer(this);
        }

        public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            if (edmType.IsEntity())
            {
                return _umbracoAnnotationsEntitySerializer;
            }

            return base.GetEdmTypeSerializer(edmType);
        }
    }
}
