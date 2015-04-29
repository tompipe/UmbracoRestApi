using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData;
using System.Web.OData.Formatter.Serialization;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Umbraco.Web.Rest.Models;

namespace Umbraco.Web.Rest.Serialization.OData
{
    // A custom serializer provider to inject the AnnotatingEntitySerializer.
    public class CustomODataSerializerProvider : DefaultODataSerializerProvider
    {
        private readonly UmbracoValidationEntitySerializer _umbracoValidationEntitySerializer;

        public CustomODataSerializerProvider()
        {
            _umbracoValidationEntitySerializer = new UmbracoValidationEntitySerializer(this);
        }

        public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            if (edmType.IsEntity())
            {
                return _umbracoValidationEntitySerializer;
            }

            return base.GetEdmTypeSerializer(edmType);
        }
    }

    public class UmbracoValidationEntitySerializer : ODataEntityTypeSerializer
    {
        public UmbracoValidationEntitySerializer(ODataSerializerProvider serializerProvider)
            : base(serializerProvider)
        {
        }

        
        //public override SelectExpandNode CreateSelectExpandNode(EntityInstanceContext entityInstanceContext)
        //{
        //    return base.CreateSelectExpandNode(entityInstanceContext);
        //}

        //public override ODataValue CreateODataValue(object graph, IEdmTypeReference expectedType, ODataSerializerContext writeContext)
        //{
        //    return base.CreateODataValue(graph, expectedType, writeContext);
        //}

        //public override ODataProperty CreateStructuralProperty(IEdmStructuralProperty structuralProperty, EntityInstanceContext entityInstanceContext)
        //{
        //    return base.CreateStructuralProperty(structuralProperty, entityInstanceContext);
        //}

        public override ODataEntry CreateEntry(SelectExpandNode selectExpandNode, EntityInstanceContext entityInstanceContext)
        {
            var entry = base.CreateEntry(selectExpandNode, entityInstanceContext);

            var content = entityInstanceContext.EntityInstance as ContentItem;
            if (entry != null && content != null)
            {
                //var currProperties = entry.Properties
                //    .Concat(new[]{new ODataProperty
                //    {
                //        Name = "properties",
                //        Value = new ODataComplexValue()
                //        {
                            
                //        }
                //    }});
                //entry.Properties = new List<ODataProperty>(currProperties);

                //// annotate the document with the score.
                //entry.InstanceAnnotations.Add(
                //    new ODataInstanceAnnotation("org.umbraco.validation.required",
                //        new ODataPrimitiveValue(true)));
            }

            return entry;
        }
    }
}
