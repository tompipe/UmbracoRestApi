using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Rest.Models
{
    public class ContentItemProperty
    {
        //NOTE: This is pretty stupid but in order for OData to serialize an 'object' it needs to be considered
        // dynamic, and for that it needs to be IDictionary<string, object>, otherwise we have to make the 
        // 'value' a String but then it wont serialize nicely
        public IDictionary<string, object> Value { get; set; }        
        public string Label { get; set; }
    }

    public class ContentItemProperties
    {
        public ContentItemProperties()
        {
            Properties = new Dictionary<string, object>();
        }

        //NOTE: needs to be 'object' for OData serialization to understand this is dynamic
        public IDictionary<string, object> Properties { get; set; }
    }

    public class ContentItem
    {
        public ContentItem()
        {
            Properties = new ContentItemProperties();
        }

        [Key]
        public int Id { get; set; }
        
        public Guid Key { get; set; }

        public string ContentTypeAlias { get; set; }

        public int ParentId { get; set; }
        //This becomes a navigation link
        public ContentItem Parent { get; set; }

        public bool HasChildren { get; set; }
        //This becomes a navigation link
        public ICollection<ContentItem> Children { get; set; } 

        public int TemplateId { get; set; }
        public int SortOrder { get; set; }
        public string Name { get; set; }
        public string UrlName { get; set; }
        public string DocumentTypeAlias { get; set; }
        public int DocumentTypeId { get; set; }
        public string WriterName { get; set; }
        public string CreatorName { get; set; }
        public int WriterId { get; set; }
        public int CreatorId { get; set; }
        public string Path { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset UpdateDate { get; set; }        
        public int Level { get; set; }
        public string Url { get; set; }        
        public PublishedItemType ItemType { get; set; }

        public ContentItemProperties Properties { get; set; }

    }
}
