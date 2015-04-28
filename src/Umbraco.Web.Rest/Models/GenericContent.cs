using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Rest.Models
{
    public class GenericContent
    {
        [Key]
        public int Id { get; set; }
        
        public Guid Key { get; set; }

        public string ContentTypeAlias { get; set; }

        public int ParentId { get; set; }
        //This becomes a navigation link
        public GenericContent Parent { get; set; }

        public bool HasChildren { get; set; }
        //This becomes a navigation link
        public ICollection<GenericContent> Children { get; set; } 

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
        
        //public ICollection<IPublishedProperty> Properties { get; private set; }

    }
}
