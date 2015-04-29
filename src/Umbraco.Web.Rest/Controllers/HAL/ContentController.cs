using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core.Services;
using Umbraco.Web.Rest.Controllers.OData;
using Umbraco.Web.Rest.Models.HAL;

namespace Umbraco.Web.Rest.Controllers.HAL
{
    public class ContentController : UmbracoHalContentController
    {

        /// <summary>
        /// Default ctor
        /// </summary>
        public ContentController()
        {
        }

        /// <summary>
        /// All dependencies
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="umbracoHelper"></param>
        public ContentController(
            UmbracoContext umbracoContext,
            UmbracoHelper umbracoHelper)
            : base(umbracoContext, umbracoHelper)
        {
        }

        public ContentListRepresentation Get()
        {
            var items = ContentService.GetRootContent();
            return Mapper.Map<ContentListRepresentation>(items);
        }
         
        public ContentRepresentation Get(int id)
        {
            var content = ContentService.GetById(id);
            if (content == null) throw new HttpResponseException(HttpStatusCode.NotFound);
            return Mapper.Map<ContentRepresentation>(content);
        }

        protected IContentService ContentService
        {
            get { return ApplicationContext.Services.ContentService; }
        }
    }
}
