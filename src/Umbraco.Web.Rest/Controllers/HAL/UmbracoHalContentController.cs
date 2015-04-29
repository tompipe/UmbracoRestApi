using Umbraco.Web.WebApi;

namespace Umbraco.Web.Rest.Controllers.HAL
{
    [UmbracoAuthorize]
    [IsBackOffice]
    [HalFormatterConfiguration]
    public class UmbracoHalContentController : UmbracoApiControllerBase
    {
        protected UmbracoHalContentController()
        {
        }

        protected UmbracoHalContentController(
            UmbracoContext umbracoContext, 
            UmbracoHelper umbracoHelper)
            : base(umbracoContext, umbracoHelper)
        {
        }
    }
}