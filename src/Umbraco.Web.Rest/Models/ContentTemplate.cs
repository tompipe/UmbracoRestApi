using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.Web.Rest.Serialization;

namespace Umbraco.Web.Rest.Models
{
    /// <summary>
    /// If the model supports creating, then this is it's template
    /// </summary>
    public class ContentTemplate
    {        
        public string ContentTypeAlias { get; set; }
        public int ParentId { get; set; }
        public int TemplateId { get; set; }
        public string Name { get; set; }

        [JsonConverter(typeof(ContentPropertyAliasJsonConverter))]
        public IDictionary<string, object> Properties { get; set; }
    }
}