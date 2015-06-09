using System;
using System.Collections.Generic;
using Umbraco.RestApi.Links;

namespace Umbraco.RestApi.Models
{
    /// <summary>
    /// Used for Content and Media
    /// </summary>
    public abstract class ContentRepresentationBase : UmbracoRepresentation
    {
        protected ContentRepresentationBase(IContentLinkTemplate<int> linkTemplate, Action<UmbracoRepresentation> createHypermediaCallback)
            : base(createHypermediaCallback)
        {
            _linkTemplate = linkTemplate;
        }

        protected ContentRepresentationBase(IContentLinkTemplate<int> linkTemplate)
        {
            _linkTemplate = linkTemplate;
        }

        protected ContentRepresentationBase(Action<UmbracoRepresentation> createHypermediaCallback)
            : base(createHypermediaCallback)
        {
        }

        protected ContentRepresentationBase()
        {
        }

        private readonly IContentLinkTemplate<int> _linkTemplate;

        public bool HasChildren { get; set; }

        public int SortOrder { get; set; }
        public string UrlName { get; set; }
        public string Url { get; set; }

        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            if (_linkTemplate == null) throw new NullReferenceException("LinkTemplate is null");

            Href = _linkTemplate.Self.CreateLink(new { id = Id }).Href;
            Rel = _linkTemplate.Self.Rel;

            Links.Add(_linkTemplate.Root.CreateLink());
            Links.Add(_linkTemplate.MetaData.CreateLink(new { id = Id }));

            if (HasChildren)
            {
                //templated links
                Links.Add(_linkTemplate.PagedChildren(Id));
                Links.Add(_linkTemplate.PagedDescendants(Id));
            }

            if (ParentId > 0)
            {
                Links.Add(_linkTemplate.Parent.CreateLink(new { parentId = ParentId }));
            }

            if (_linkTemplate.Upload != null)
            {
                Links.Add(_linkTemplate.Upload.CreateLink(new { id = Id }));
            }
            
        }
    }

}