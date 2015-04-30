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
        private readonly IContentLinkTemplate _linkTemplate;       

        public ContentRepresentation()
        {
            Properties = new Dictionary<string, ContentPropertyRepresentation>();
        }

        public ContentRepresentation(IContentLinkTemplate linkTemplate)
        {
            _linkTemplate = linkTemplate;
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
                if (_linkTemplate == null) throw new NullReferenceException("LinkTemplate is null");
                return _linkTemplate.ContentItem.Rel;
            }
            set { }
        }

        public override string Href
        {
            get
            {
                if (_linkTemplate == null) throw new NullReferenceException("LinkTemplate is null");
                return _linkTemplate.ContentItem.CreateLink(new { id = Id }).Href;
            }
            set { }
        }

        protected override void CreateHypermedia()
        {
            if (_linkTemplate == null) throw new NullReferenceException("LinkTemplate is null");

            Links.Add(_linkTemplate.RootContent.CreateLink());

            if (HasChildren)
            {
                Links.Add(_linkTemplate.ChildContent.CreateLink(new { id = Id }));
            }

            if (ParentId > 0)
            {
                Links.Add(_linkTemplate.ParentContent.CreateLink(new { parentId = ParentId }));
            }
        }
    }
}
