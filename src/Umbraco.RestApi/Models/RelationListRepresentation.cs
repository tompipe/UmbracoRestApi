using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.RestApi.Links;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class RelationListRepresentation : SimpleListRepresentation<RelationRepresentation>
    {
        private readonly RelationLinkTemplate _linkTemplate;

        public RelationListRepresentation(IList<RelationRepresentation> res, RelationLinkTemplate linkTemplate)
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

        }
    }
}
