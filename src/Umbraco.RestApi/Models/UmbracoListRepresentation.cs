using System.Collections.Generic;
using Umbraco.RestApi.Links;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class UmbracoListRepresentation<TRepresentation> : SimpleListRepresentation<TRepresentation>
        where TRepresentation: UmbracoRepresentationBase
    {
        private readonly ILinkTemplate _linkTemplate;

        public UmbracoListRepresentation(IList<TRepresentation> res, ILinkTemplate linkTemplate)
            : base(res)
        {
            _linkTemplate = linkTemplate;
            TotalResults = res.Count;
        }

        public int TotalResults { get; set; }

        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            Links.Add(_linkTemplate.Root.CreateLink());

            //templated link
            Links.Add(_linkTemplate.Search);
        }
    }
}