using System.Collections.Generic;
using Umbraco.RestApi.Links;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class PagedMemberListRepresentation<TRepresentation> : PagedRepresentationList<TRepresentation>
        where TRepresentation : Representation
    {
        private readonly ILinkTemplate _linkTemplate;


        public PagedMemberListRepresentation(IList<TRepresentation> res, long totalResults, long totalPages, long pageIndex, int pageSize, ILinkTemplate linkTemplate, Link pagedUriTemplate, object uriTemplateSubstitutionParams)
            : base(res, totalResults, totalPages, pageIndex, pageSize, pagedUriTemplate, uriTemplateSubstitutionParams)
        {
            _linkTemplate = linkTemplate;
        }

        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            //add templated root link
            Links.Add(_linkTemplate.Root);

            //templated link
            Links.Add(_linkTemplate.Search);
        }
    }
}