using System;
using Umbraco.RestApi.Links;

namespace Umbraco.RestApi.Models
{
    public class MediaRepresentation : ContentRepresentationBase
    {
        public MediaRepresentation(IContentLinkTemplate linkTemplate, Action<UmbracoRepresentation> createHypermediaCallback)
            : base(linkTemplate, createHypermediaCallback)
        {
        }

        public MediaRepresentation(IContentLinkTemplate linkTemplate)
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