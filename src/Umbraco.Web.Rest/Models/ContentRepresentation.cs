using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Umbraco.Core.Models;
using Umbraco.Web.Rest.Links;
using Umbraco.Web.Rest.Serialization;
using WebApi.Hal;

namespace Umbraco.Web.Rest.Models
{
    public class ContentRepresentation : Representation
    {
        [JsonIgnore]
        public IContentLinkTemplate LinkTemplate { get; set; }

        public ContentRepresentation()
        {
            Properties = new Dictionary<string, ContentPropertyRepresentation>();
        }

        public ContentRepresentation(IContentLinkTemplate linkTemplate)
        {
            LinkTemplate = linkTemplate;
        }

        public int Id { get; set; }
        public Guid Key { get; set; }
        [Required]
        public string ContentTypeAlias { get; set; }
        [Required]
        public int ParentId { get; set; }
        public bool HasChildren { get; set; }
        [Required]
        public int TemplateId { get; set; }
        public int SortOrder { get; set; }
        [Required]
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

        [JsonConverter(typeof(ContentPropertyAliasJsonConverter))]
        public IDictionary<string, ContentPropertyRepresentation> Properties { get; set; }
        
        public override string Rel
        {
            get
            {
                if (LinkTemplate == null) throw new NullReferenceException("LinkTemplate is null");
                return LinkTemplate.ContentItem.Rel;
            }
            set { }
        }

        public override string Href
        {
            get
            {
                if (LinkTemplate == null) throw new NullReferenceException("LinkTemplate is null");
                return LinkTemplate.ContentItem.CreateLink(new { id = Id }).Href;
            }
            set { }
        }

        protected override void CreateHypermedia()
        {
            if (LinkTemplate == null) throw new NullReferenceException("LinkTemplate is null");

            Links.Add(LinkTemplate.RootContent.CreateLink());

            if (HasChildren)
            {
                Links.Add(LinkTemplate.ChildContent.CreateLink(new { id = Id }));
            }

            if (ParentId > 0)
            {
                Links.Add(LinkTemplate.ParentContent.CreateLink(new { parentId = ParentId }));
            }
        }
    }
}
