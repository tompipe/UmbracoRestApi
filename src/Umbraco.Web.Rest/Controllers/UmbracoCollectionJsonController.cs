using Umbraco.Web.WebApi;

namespace Umbraco.Web.Rest.Controllers
{
    /// <summary>
    /// Base controller for Umbraco REST API controllers
    /// </summary>
    [UmbracoAuthorize]
    [IsBackOffice]
    [CollectionJsonFormatterConfiguration]
    public abstract class UmbracoCollectionJsonController : UmbracoApiControllerBase
    {
        protected UmbracoCollectionJsonController()
        {
        }

        protected UmbracoCollectionJsonController(UmbracoContext umbracoContext) : base(umbracoContext)
        {
        }
    }
}