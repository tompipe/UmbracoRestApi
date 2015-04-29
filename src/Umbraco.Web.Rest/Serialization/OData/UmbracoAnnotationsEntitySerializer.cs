using System.Linq;
using System.Web.OData;
using System.Web.OData.Formatter.Serialization;
using Microsoft.OData.Core;
using Umbraco.Web.Rest.Models;

namespace Umbraco.Web.Rest.Serialization.OData
{
    /// <summary>
    /// Custom serializer to add annotations to the output
    /// </summary>
    public class UmbracoAnnotationsEntitySerializer : ODataEntityTypeSerializer
    {
        public UmbracoAnnotationsEntitySerializer(ODataSerializerProvider serializerProvider)
            : base(serializerProvider)
        {
        }
        
        public override ODataEntry CreateEntry(SelectExpandNode selectExpandNode, EntityInstanceContext entityInstanceContext)
        {
            var entry = base.CreateEntry(selectExpandNode, entityInstanceContext);

            var content = entityInstanceContext.EntityInstance as ContentItem;
            if (entry != null && content != null)
            {
                var propertiesValue = (ODataComplexValue)entry.Properties.Single(x => x.Name == "properties").Value;
                foreach (var property in content.Properties.Properties)
                {
                    var propertyType = ((ContentItemProperty) property.Value).PropertyType;

                    var propertyValue = propertiesValue.Properties.Single(x => x.Name == propertyType.Alias);
                    //TODO: adding an annotation to the result BUT it doesn't show up. Lots of people reporting this online too:
                    // http://stackoverflow.com/questions/23519634/instanceannotation-into-odata-response
                    propertyValue.InstanceAnnotations.Add(new ODataInstanceAnnotation("org.umbraco.validationRequired", new ODataPrimitiveValue(propertyType.Mandatory)));

                }
            }

            return entry;
        }
    }
}