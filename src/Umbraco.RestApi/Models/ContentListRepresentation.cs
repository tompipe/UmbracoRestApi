using System.Collections.Generic;
using Umbraco.RestApi.Links;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class ContentListRepresentation : SimpleListRepresentation<ContentRepresentation>
    {
        private readonly IContentLinkTemplate _linkTemplate;

        public ContentListRepresentation(IList<ContentRepresentation> res, IContentLinkTemplate linkTemplate)
            : base(res)
        {
            _linkTemplate = linkTemplate;
            TotalResults = res.Count;
        }

        public int TotalResults { get; set; }

        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            Links.Add(_linkTemplate.RootContent.CreateLink());

            //templated link
            Links.Add(_linkTemplate.Search);
        }
    }
}