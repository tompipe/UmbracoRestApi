using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Umbraco.Web.Rest.Links;
using Umbraco.Web.Rest.Serialization;
using WebApi.Hal;

namespace Umbraco.Web.Rest.Models
{
    /// <summary>
    /// A validation error response
    /// </summary>
    /// <remarks>
    /// Validation responses are done with the vnd.error standard, some info can be found here: https://github.com/blongden/vnd.error
    /// </remarks>
    public class ValidationErrorListRepresentation : SimpleListRepresentation<ValidationErrorRepresentation>
    {
        private readonly IContentLinkTemplate _linkTemplate;
        private readonly int? _id;

        public ValidationErrorListRepresentation(IList<ValidationErrorRepresentation> res, IContentLinkTemplate linkTemplate, int? id = null)
            : base(res)
        {
            _linkTemplate = linkTemplate;
            _id = id;
            TotalResults = res.Count;
        }

        public int TotalResults { get; set; }

        public string Message { get; set; }        
        public string LogRef { get; set; }        
        public int HttpStatus { get; set; }

        public override string Rel
        {
            get
            {
                if (_linkTemplate == null) throw new NullReferenceException("LinkTemplate is null");
                return _linkTemplate.ContentItem.Rel;
            }
            set { }
        }

        public override string Href
        {
            get
            {
                if (_linkTemplate == null) throw new NullReferenceException("LinkTemplate is null");
                
                return _id.HasValue
                    ? _linkTemplate.ContentItem.CreateLink(new { id = _id.Value }).Href
                    //Same as 'root' content for Post
                    : _linkTemplate.RootContent.CreateLink().Href;
            }
            set { }
        }
    }
}
