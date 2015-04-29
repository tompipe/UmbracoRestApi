using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;

namespace Umbraco.Web.Rest.Models
{
    public class ContentItemProperty
    {
        //NOTE: This is pretty stupid but in order for OData to serialize an 'object' it needs to be considered
        // dynamic, and for that it needs to be IDictionary<string, object>, otherwise we have to make the 
        // 'value' a String but then it wont serialize nicely
        [Required]
        public IDictionary<string, object> Value { get; set; }        

        public string Label { get; set; }

        internal PropertyType PropertyType { get; set; }
    }
}