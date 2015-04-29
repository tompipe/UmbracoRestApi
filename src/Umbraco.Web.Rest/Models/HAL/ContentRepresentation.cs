using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Web.Rest.Models.OData;
using WebApi.Hal;

namespace Umbraco.Web.Rest.Models.HAL
{
    public class ContentRepresentation : Representation
    {
        public ContentRepresentation()
        {
            Properties = new Dictionary<string, ContentPropertyRepresentation>();
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

        public IDictionary<string, ContentPropertyRepresentation> Properties { get; set; }


        public override string Rel
        {
            get { return LinkTemplates.Content.ContentItem.Rel; }
            set { }
        }

        public override string Href
        {
            get { return LinkTemplates.Content.ContentItem.CreateLink(new { id = Id }).Href; }
            set { }
        }

        protected override void CreateHypermedia()
        {
            if (HasChildren)
            {
                Links.Add(LinkTemplates.Content.ChildContent.CreateLink(new { id = Id }));
            }

            if (ParentId > 0)
            {
                Links.Add(LinkTemplates.Content.ParentContent.CreateLink(new { parentId = ParentId }));
            }
        }
    }
}
