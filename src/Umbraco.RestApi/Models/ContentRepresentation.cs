using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Umbraco.Core.Models;
using Umbraco.RestApi.Links;
using Umbraco.RestApi.Serialization;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class ContentRepresentation : Representation
    {
        private readonly IContentLinkTemplate _linkTemplate;       

        public ContentRepresentation()
        {
            Properties = new Dictionary<string, object>();
        }

        public ContentRepresentation(IContentLinkTemplate linkTemplate)
        {
            _linkTemplate = linkTemplate;
        }

        public int Id { get; set; }
        public Guid Key { get; set; }
        
        [Required]
        [Display(Name = "contentTypeAlias")]  
        public string ContentTypeAlias { get; set; }
        [Required]
        [Display(Name = "parentId")]
        public int ParentId { get; set; }
        public bool HasChildren { get; set; }
        [Required]
        [Display(Name = "templateId")]
        public int TemplateId { get; set; }
        public int SortOrder { get; set; }
        [Required]
        [Display(Name = "name")]
        public string Name { get; set; }
        public string UrlName { get; set; }
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

        [JsonConverter(typeof(ExplicitlyCasedDictionaryKeyJsonConverter<object>))]
        public IDictionary<string, object> Properties { get; set; }
      
        protected override void CreateHypermedia()
        {
            if (_linkTemplate == null) throw new NullReferenceException("LinkTemplate is null");

            Href = _linkTemplate.ContentItem.CreateLink(new {id = Id}).Href;
            Rel = _linkTemplate.ContentItem.Rel;

            Links.Add(_linkTemplate.RootContent.CreateLink());
            Links.Add(_linkTemplate.ContentMetaData.CreateLink(new { id = Id }));
            
            if (HasChildren)
            {
                //templated links
                Links.Add(_linkTemplate.PagedChildContent);
                Links.Add(_linkTemplate.PagedDescendantContent);
            }

            if (ParentId > 0)
            {
                Links.Add(_linkTemplate.ParentContent.CreateLink(new { parentId = ParentId }));
            }
        }
    }
}
