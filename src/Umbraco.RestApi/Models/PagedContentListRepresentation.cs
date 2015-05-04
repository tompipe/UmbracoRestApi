using System.Collections.Generic;
using Umbraco.RestApi.Links;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class PagedContentListRepresentation : PagedRepresentationList<ContentRepresentation>
    {
        private readonly IContentLinkTemplate _linkTemplate;


        public PagedContentListRepresentation(IList<ContentRepresentation> res, long totalResults, long totalPages, long pageIndex, int pageSize, IContentLinkTemplate linkTemplate, Link pagedUriTemplate, object uriTemplateSubstitutionParams)
            : base(res, totalResults, totalPages, pageIndex, pageSize, pagedUriTemplate, uriTemplateSubstitutionParams)
        {
            _linkTemplate = linkTemplate;
        }

        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            Links.Add(_linkTemplate.RootContent.CreateLink());
        }
    }
}