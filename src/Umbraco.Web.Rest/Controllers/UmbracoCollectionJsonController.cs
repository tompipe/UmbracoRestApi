using System;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Web.Security;
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

        protected UmbracoCollectionJsonController(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper)
            : base(umbracoContext, umbracoHelper)
        {
        }

        /// <summary>
        /// Get a collection of root nodes
        /// </summary>
        /// <returns></returns>
        [Route("")]
        public abstract HttpResponseMessage Get();

        /// <summary>
        /// Returns a single item with traversal rels
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id:int}")]
        public abstract HttpResponseMessage Get(int id);

        /// <summary>
        /// Returns the children for an item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id:int}/children")]
        public abstract HttpResponseMessage GetChildren(int id);

    }

}