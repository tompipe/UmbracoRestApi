using System;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.RestApi.Models
{
    public abstract class UmbracoRepresentation : UmbracoRepresentationBase
    {
        private readonly Action<UmbracoRepresentation> _createHypermediaCallback;

        protected UmbracoRepresentation()
        {
            
        }
        protected UmbracoRepresentation(Action<UmbracoRepresentation> createHypermediaCallback)
        {
            _createHypermediaCallback = createHypermediaCallback;
        }

        [Required]
        [Display(Name = "contentTypeAlias")]
        public string ContentTypeAlias { get; set; }

        [Required]
        [Display(Name = "parentId")]
        public int ParentId { get; set; }

        public string WriterName { get; set; }
        public string CreatorName { get; set; }
        public int WriterId { get; set; }
        public int CreatorId { get; set; }
        public string Path { get; set; }
        public int Level { get; set; }

        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();

            if (_createHypermediaCallback != null)
            {
                _createHypermediaCallback(this);
            }
        }
    }
}