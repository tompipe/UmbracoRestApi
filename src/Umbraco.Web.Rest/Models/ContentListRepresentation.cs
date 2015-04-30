using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Web.Rest.Links;
using WebApi.Hal;

namespace Umbraco.Web.Rest.Models
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

            //TODO: Add search
            //var search = LinkTemplates.Beers.SearchBeers;
            //if (Links.Count(l=>l.Rel == search.Rel && l.Href == search.Href) == 0)
            //    Links.Add(LinkTemplates.Beers.SearchBeers);
        }
    }
}