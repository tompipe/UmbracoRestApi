using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Examine;
using Examine.Providers;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.RestApi.Links;
using Umbraco.RestApi.Models;
using Umbraco.RestApi.Routing;
using Umbraco.Web;

namespace Umbraco.RestApi.Controllers
{
    [UmbracoRoutePrefix("rest/v1/media")]
    public class MediaController : UmbracoHalContentControllerBase<int, IMedia, MediaRepresentation>
    {


        /// <summary>
        /// Default ctor
        /// </summary>
        public MediaController()
        {
        }

        /// <summary>
        /// All dependencies
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="umbracoHelper"></param>
        /// <param name="searchProvider"></param>
        public MediaController(
            UmbracoContext umbracoContext,
            UmbracoHelper umbracoHelper,
            BaseSearchProvider searchProvider)
            : base(umbracoContext, umbracoHelper)
        {
            if (searchProvider == null) throw new ArgumentNullException("searchProvider");
            _searchProvider = searchProvider;
        }

        private BaseSearchProvider _searchProvider;
        protected BaseSearchProvider SearchProvider
        {
            get { return _searchProvider ?? (_searchProvider = ExamineManager.Instance.SearchProviderCollection["InternalSearcher"]); }
        }

        protected override PagedResult<IMedia> PerformSearch(QueryStructure query)
        {
            if (query.Lucene.IsNullOrWhiteSpace()) throw new HttpResponseException(HttpStatusCode.NotFound);

            var result =
                SearchProvider.Search(
                    SearchProvider.CreateSearchCriteria().RawQuery(query.Lucene),
                    query.PageSize);

            var paged = result.Skip(GetSkipSize(query.PageIndex, query.PageSize)).ToArray();

            //TODO: We really need to make a model mapper from search result to IMedia, for now well just go lookup that content :(

            if (paged.Any())
            {
                var foundContent = MediaService.GetByIds(paged.Select(x => x.Id)).WhereNotNull();

                return new PagedResult<IMedia>(result.TotalItemCount, query.PageIndex + 1, query.PageSize)
                {
                    Items = foundContent
                };
            }

            return new PagedResult<IMedia>(result.TotalItemCount, query.PageIndex + 1, query.PageSize)
            {
                Items = Enumerable.Empty<IMedia>()
            };

        }

        protected override IEnumerable<IMedia> GetRootContent()
        {
            return MediaService.GetRootMedia();
        }

        protected override ContentMetadataRepresentation GetMetadataForItem(int id)
        {
            var found = MediaService.GetById(id);
            if (found == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            var result = new ContentMetadataRepresentation(LinkTemplate, id)
            {
                Fields = GetDefaultFieldMetaData(),
                Properties = Mapper.Map<IDictionary<string, ContentPropertyInfo>>(found),
                CreateTemplate = Mapper.Map<ContentTemplate>(found)
            };
            return result;
        }

        protected override IMedia GetItem(int id)
        {
            return MediaService.GetById(id);
        }

        protected override PagedResult<IMedia> GetChildContent(int id, long pageIndex = 0, int pageSize = 100)
        {
            long total;
            var items = MediaService.GetPagedChildren(id, pageIndex, pageSize, out total);
            return new PagedResult<IMedia>(total, pageIndex + 1, pageSize)
            {
                Items = items
            };
        }

        protected override PagedResult<IMedia> GetDescendantContent(int id, long pageIndex = 0, int pageSize = 100)
        {
            long total;
            var items = MediaService.GetPagedDescendants(id, pageIndex, pageSize, out total);
            return new PagedResult<IMedia>(total, pageIndex + 1, pageSize)
            {
                Items = items
            };
        }

        protected override IMedia CreateNew(MediaRepresentation content)
        {
            //we cannot continue here if the mandatory items are empty (i.e. name, etc...)
            if (!ModelState.IsValid)
            {
                throw ValidationException(ModelState, content);
            }

            var contentType = Services.ContentTypeService.GetMediaType(content.ContentTypeAlias);
            if (contentType == null)
            {
                ModelState.AddModelError("content.contentTypeAlias", "No media type found with alias " + content.ContentTypeAlias);
                throw ValidationException(ModelState, content);
            }

            //create an item before persisting of the correct content type
            var created = MediaService.CreateMedia(content.Name, content.ParentId, content.ContentTypeAlias, Security.CurrentUser.Id);

            //Validate properties
            var validator = new ContentPropertyValidator<IMedia>(ModelState, Services.DataTypeService);
            validator.ValidateItem(content, created);

            if (!ModelState.IsValid)
            {
                throw ValidationException(ModelState, content);
            }

            Mapper.Map(content, created);

            MediaService.Save(created);

            return created;
        }

        protected override IMedia Update(int id, MediaRepresentation content)
        {
            var found = MediaService.GetById(id);
            if (found == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            //Validate properties
            var validator = new ContentPropertyValidator<IMedia>(ModelState, Services.DataTypeService);
            validator.ValidateItem(content, found);

            if (!ModelState.IsValid)
            {
                throw ValidationException(ModelState, content, id: id);
            }

            Mapper.Map(content, found);

            MediaService.Save(found);

            return found;
        }

        protected override IContentLinkTemplate<int> LinkTemplate
        {
            get { return new MediaLinkTemplate(CurrentVersionRequest); }
        }

        /// <summary>
        /// Creates the content representation from the entity based on the current API version
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected override MediaRepresentation CreateRepresentation(IMedia entity)
        {
            //create it with the current version link representation
            var representation = new MediaRepresentation(LinkTemplate);
            return Mapper.Map(entity, representation);
        }

        protected IMediaService MediaService
        {
            get { return ApplicationContext.Services.MediaService; }
        }
    }

}
