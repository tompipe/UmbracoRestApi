using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        /// <summary>
        /// Returns the children for an item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("children")]
        public ContentListRepresentation GetChildren(int id)
        {
            var items = ContentService.GetChildren(id);
            return Mapper.Map<ContentListRepresentation>(items);
        }

        public HttpResponseMessage Post(ContentRepresentation content)
        {
            return Request.CreateResponse(HttpStatusCode.Created, content);
        }


        public ContentRepresentation Put(int id, string value)
        {
            var content = ContentService.GetById(id);
            if (content == null) throw new HttpResponseException(HttpStatusCode.NotFound);
            //TODO: Update
            return Mapper.Map<ContentRepresentation>(content);
        }

        public void Delete(int id)
        {
        }

        protected IContentService ContentService
        {
            get { return ApplicationContext.Services.ContentService; }
        }
    }
}
