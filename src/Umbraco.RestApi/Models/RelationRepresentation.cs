using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models.EntityBase;
using Umbraco.RestApi.Links;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class RelationRepresentation : Representation
    {
        private readonly ILinkTemplate _parentLink;
        private readonly ILinkTemplate _childLink;
        private readonly IRelationLinkTemplate _relationLink;

        public RelationRepresentation(IRelationLinkTemplate linktemplate, ILinkTemplate parentLinkTemplate, ILinkTemplate childLinkTemplate)
        {
            _parentLink = parentLinkTemplate;
            _childLink = childLinkTemplate;
            _relationLink = linktemplate;
        }

        public int Id { get; set; }

        public int ChildId { get; set; }
        public int ParentId { get; set; }

        public string Comment { get; set; }
        public DateTime CreateDate { get; set; }
        public string RelationTypeAlias { get; set; }

        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            //link to self
            Href = _relationLink.ContentItem.CreateLink(new { id = Id }).Href;
            Rel = _relationLink.ContentItem.Rel;

            if (_parentLink != null)
                Links.Add(_parentLink.Self.CreateLink("parent", new { id = ParentId }));

            if (_childLink != null)
                Links.Add(_childLink.Self.CreateLink("child", new { id = ChildId }));

        }


    }
}
