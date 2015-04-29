using System.Collections.Generic;
using WebApi.Hal;

namespace Umbraco.Web.Rest.Models.HAL
{
    public class ContentListRepresentation : SimpleListRepresentation<ContentRepresentation>
    {
        public ContentListRepresentation(IList<ContentRepresentation> res)
            : base(res)
        {
        }
    }
}