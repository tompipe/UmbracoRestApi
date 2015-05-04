using System.Collections.Generic;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public abstract class PagedRepresentationList<TRepresentation> : SimpleListRepresentation<TRepresentation> where TRepresentation : Representation
    {
        readonly Link _uriTemplate;

        protected PagedRepresentationList(IList<TRepresentation> res, long totalResults, long totalPages, long pageIndex, int pageSize, Link uriTemplate, object uriTemplateSubstitutionParams)
            : base(res)
        {
            this._uriTemplate = uriTemplate;
            PageSize = pageSize;
            TotalResults = totalResults;
            TotalPages = totalPages;
            PageIndex = pageIndex;
            UriTemplateSubstitutionParams = uriTemplateSubstitutionParams;
        }

        public long TotalResults { get; set; }
        public long TotalPages { get; set; }
        public long PageIndex { get; set; }
        public int PageSize { get; set; }

        protected object UriTemplateSubstitutionParams;

        protected override void CreateHypermedia()
        {
            var prms = new List<object>
            {
                new
                {
                    pageIndex = PageIndex,
                    pageSize = PageSize
                }
            };

            if (UriTemplateSubstitutionParams != null)
                prms.Add(UriTemplateSubstitutionParams);

            Href = Href ?? _uriTemplate.CreateLink(prms.ToArray()).Href;

            Links.Add(new Link { Href = Href, Rel = "self" });

            if (PageIndex > 0)
            {
                var item = UriTemplateSubstitutionParams == null
                    ? _uriTemplate.CreateLink("prev", new { pageIndex = PageIndex - 1, pageSize = PageSize })
                    : _uriTemplate.CreateLink("prev", UriTemplateSubstitutionParams, new { pageIndex = PageIndex - 1, pageSize = PageSize }); // page overrides UriTemplateSubstitutionParams
                Links.Add(item);
            }
            if (PageIndex < (TotalPages -1))
            {
                var link = UriTemplateSubstitutionParams == null // kbr
                    ? _uriTemplate.CreateLink("next", new { page = PageIndex + 1, pageSize = PageSize })
                    : _uriTemplate.CreateLink("next", UriTemplateSubstitutionParams, new { pageIndex = PageIndex + 1, pageSize = PageSize }); // page overrides UriTemplateSubstitutionParams
                Links.Add(link);
            }

        }
    }
}