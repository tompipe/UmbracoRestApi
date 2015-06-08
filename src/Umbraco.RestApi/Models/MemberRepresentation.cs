using System;
using System.ComponentModel.DataAnnotations;
using Umbraco.RestApi.Links;

namespace Umbraco.RestApi.Models
{
    public class MemberRepresentation : UmbracoRepresentation
    {
        public MemberRepresentation(ILinkTemplate linkTemplate, Action<UmbracoRepresentation> createHypermediaCallback)
            : base(createHypermediaCallback)
        {
            _linkTemplate = linkTemplate;
        }

        public MemberRepresentation(Action<UmbracoRepresentation> createHypermediaCallback)
            : base(createHypermediaCallback)
        {
        }

        public MemberRepresentation()
        {
        }

        public MemberRepresentation(ILinkTemplate linkTemplate)
        {
            _linkTemplate = linkTemplate;
        }

        [Required]
        [Display(Name = "userName")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "email")]
        public string Email { get; set; }

        private readonly ILinkTemplate _linkTemplate;

        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            if (_linkTemplate == null) throw new NullReferenceException("LinkTemplate is null");

            Href = _linkTemplate.Self.CreateLink(new { id = Id }).Href;
            Rel = _linkTemplate.Self.Rel;

            Links.Add(_linkTemplate.MetaData.CreateLink(new { id = Id }));
        }
    }
}