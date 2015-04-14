using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CollectionJson;
using CollectionJson.Server;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Web.Rest.Controllers
{
    public class ContentController : UmbracoCollectionJsonController<IContent, int>
    {
        private readonly IContentService _contentService;

        public ContentController()
        {
        }

        public ContentController(
            UmbracoContext umbracoContext, 
            UmbracoHelper umbracoHelper, 
            ICollectionJsonDocumentWriter<IContent> writer, 
            ICollectionJsonDocumentReader<IContent> reader)
            : base(umbracoContext, umbracoHelper, writer, reader)
        {
            _contentService = umbracoContext.Application.Services.ContentService;
        }
    }
}
