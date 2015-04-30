using System;
using System.Collections.Generic;
using Umbraco.Web.Rest.Links;
using WebApi.Hal;

namespace Umbraco.Web.Rest.Models
{
    public class ContentMetadataRepresentation : Representation
    {
        private readonly IContentLinkTemplate _linkTemplate;       

        public ContentMetadataRepresentation(IContentLinkTemplate linkTemplate, int id)
        {
            _linkTemplate = linkTemplate;
            Id = id;
            Fields = new Dictionary<string, FieldInfo>();
            Properties = new Dictionary<string, FieldInfo>();
        }

        public int Id { get; set; }
        public Guid Key { get; set; }

        /// <summary>
        /// If the model supports creating, then this is it's template, null means it does not support creating
        /// </summary>
        public ContentTemplate CreateTemplate { get; set; }

        public IDictionary<string, FieldInfo> Fields { get; set; }
        public IDictionary<string, FieldInfo> Properties { get; set; }

        public override string Rel
        {
            get
            {
                if (_linkTemplate == null) throw new NullReferenceException("LinkTemplate is null");
                return _linkTemplate.ContentMetaData.Rel;
            }
            set { throw new NotSupportedException(); }
        }

        public override string Href
        {
            get
            {
                if (_linkTemplate == null) throw new NullReferenceException("LinkTemplate is null");
                return _linkTemplate.ContentMetaData.CreateLink(new { id = Id }).Href;
            }
            set { throw new NotSupportedException(); }
        }

        protected override void CreateHypermedia()
        {
            if (_linkTemplate == null) throw new NullReferenceException("LinkTemplate is null");
            Links.Add(_linkTemplate.ContentItem.CreateLink(new { id = Id }));      
        }
    }
}