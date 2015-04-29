using System;
using System.Web.Http.Description;
using System.Web.OData;
using System.Web.OData.Results;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Rest.Controllers.OData
{
    [UmbracoAuthorize]
    [IsBackOffice]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ODataFormatting]
    [ODataRouting]
    [UmbracoODataFormatterConfiguration]
    public abstract class UmbracoODataController : UmbracoApiControllerBase
    {
        
        protected UmbracoODataController()
        {
        }

        protected UmbracoODataController(
            UmbracoContext umbracoContext, 
            UmbracoHelper umbracoHelper)
            : base(umbracoContext, umbracoHelper)
        {
            
        }

        /// <summary>
        /// Creates an action result with the specified values that is a response to a POST operation with an entity to an entity set.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Web.OData.Results.CreatedODataResult`1"/> with the specified values.
        /// </returns>
        /// <param name="entity">The created entity.</param><typeparam name="TEntity">The created entity type.</typeparam>
        protected virtual CreatedODataResult<TEntity> Created<TEntity>(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new CreatedODataResult<TEntity>(entity, this);
        }

        /// <summary>
        /// Creates an action result with the specified values that is a response to a PUT, PATCH, or a MERGE operation on an OData entity.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.Web.OData.Results.UpdatedODataResult`1"/> with the specified values.
        /// </returns>
        /// <param name="entity">The updated entity.</param><typeparam name="TEntity">The updated entity type.</typeparam>
        protected virtual UpdatedODataResult<TEntity> Updated<TEntity>(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new UpdatedODataResult<TEntity>(entity, this);
        }
    }
}