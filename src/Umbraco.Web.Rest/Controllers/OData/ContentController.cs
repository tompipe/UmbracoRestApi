using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using CollectionJson;
using Umbraco.Core.Services;
using Umbraco.Web.Rest.Models;

namespace Umbraco.Web.Rest.Controllers.OData
{
    public class ContentController : UmbracoODataController
    {
        public ContentController()
        {
        }

        public ContentController(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper)
            : base(umbracoContext, umbracoHelper)
        {
        }


        [EnableQuery]
        public GenericContent Get([FromODataUri] int key)
        {
            var content = ContentService.GetById(key);

            return new GenericContent
            {
                Id = content.Id,
                Name = content.Name,
                Level = content.Level,
                ParentId = content.ParentId
            };
        }

        protected IContentService ContentService
        {
            get { return UmbracoContext.Application.Services.ContentService; }
        }

        //[EnableQuery]
        //public IQueryable<GenericContent> GetProducts()
        //{
        //    throw new NotImplementedException();
        //}

        //[EnableQuery]
        //public SingleResult<GenericContent> GetProduct([FromODataUri] int key)
        //{
        //    throw new NotImplementedException();
        //}

    }
}
