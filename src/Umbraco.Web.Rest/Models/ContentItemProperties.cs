using System.Collections.Generic;

namespace Umbraco.Web.Rest.Models
{
    public class ContentItemProperties
    {
        public ContentItemProperties()
        {
            Properties = new Dictionary<string, object>();
        }

        //NOTE: needs to be 'object' for OData serialization to understand this is dynamic
        public IDictionary<string, object> Properties { get; set; }
    }
}