using System.Collections.Generic;
using Umbraco.RestApi.Links;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class PagedContentListRepresentation<TRepresentation> : PagedRepresentationList<TRepresentation> 
        where TRepresentation : Representation
    {
        private readonly IContentLinkTemplate _linkTemplate;


        public PagedContentListRepresentation(IList<TRepresentation> res, long totalResults, long totalPages, long pageIndex, int pageSize, IContentLinkTemplate linkTemplate, Link pagedUriTemplate, object uriTemplateSubstitutionParams)
            : base(res, totalResults, totalPages, pageIndex, pageSize, pagedUriTemplate, uriTemplateSubstitutionParams)
        {
            _linkTemplate = linkTemplate;
        }

        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            //add root
            Links.Add(_linkTemplate.Root.CreateLink());

            //templated link
            Links.Add(_linkTemplate.Search);
        }
    }
}