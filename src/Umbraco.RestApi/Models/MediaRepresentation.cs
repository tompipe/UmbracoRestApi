using System;
using Umbraco.RestApi.Links;

namespace Umbraco.RestApi.Models
{
    public class MediaRepresentation : ContentRepresentationBase
    {
        public MediaRepresentation(IContentLinkTemplate<int> linkTemplate, Action<UmbracoRepresentation> createHypermediaCallback)
            : base(linkTemplate, createHypermediaCallback)
        {
        }

        public MediaRepresentation(IContentLinkTemplate<int> linkTemplate)
            : base(linkTemplate)
        {
        }

        public MediaRepresentation(Action<UmbracoRepresentation> createHypermediaCallback)
            : base(createHypermediaCallback)
        {
        }

        public MediaRepresentation()
        {
        }
    }
}