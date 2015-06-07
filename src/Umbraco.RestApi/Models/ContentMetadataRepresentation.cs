using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.RestApi.Links;
using Umbraco.RestApi.Serialization;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class ContentMetadataRepresentation : Representation
    {
        private readonly ILinkTemplate _linkTemplate;       

        public ContentMetadataRepresentation(ILinkTemplate linkTemplate, int id)
        {
            _linkTemplate = linkTemplate;
            Id = id;
            Fields = new Dictionary<string, ContentPropertyInfo>();
            Properties = new Dictionary<string, ContentPropertyInfo>();
        }

        public int Id { get; set; }
        public Guid Key { get; set; }

        /// <summary>
        /// If the model supports creating, then this is it's template, null means it does not support creating
        /// </summary>
        public ContentTemplate CreateTemplate { get; set; }

        public IDictionary<string, ContentPropertyInfo> Fields { get; set; }

        [JsonConverter(typeof(ExplicitlyCasedDictionaryKeyJsonConverter<ContentPropertyInfo>))]
        public IDictionary<string, ContentPropertyInfo> Properties { get; set; }

        public override string Rel
        {
            get
            {
                if (_linkTemplate == null) throw new NullReferenceException("LinkTemplate is null");
                return _linkTemplate.MetaData.Rel;
            }
            set { throw new NotSupportedException(); }
        }

        public override string Href
        {
            get
            {
                if (_linkTemplate == null) throw new NullReferenceException("LinkTemplate is null");
                return _linkTemplate.MetaData.CreateLink(new { id = Id }).Href;
            }
            set { throw new NotSupportedException(); }
        }

        protected override void CreateHypermedia()
        {
            if (_linkTemplate == null) throw new NullReferenceException("LinkTemplate is null");
            Links.Add(_linkTemplate.Self.CreateLink(new { id = Id }));      
        }
    }
}