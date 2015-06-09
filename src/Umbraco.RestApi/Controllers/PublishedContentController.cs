using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// <summary>
    /// REST service for querying against Published content
    /// </summary>    
    [UmbracoRoutePrefix("rest/v1/content/published")]
    public class PublishedContentController : UmbracoHalContentControllerBase<int, IPublishedContent, ContentRepresentation>
    {
        //TODO: We need to make a way to return IPublishedContent from either the cache or from Examine, then convert that to the output
        // this controller needs to support both data sources in one way or another - either base classes, etc...

        /// <summary>
        /// Default ctor
        /// </summary>
        public PublishedContentController()
        {
        }

        /// <summary>
        /// All dependencies
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="umbracoHelper"></param>
        /// <param name="searchProvider"></param>
        public PublishedContentController(
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
            get { return _searchProvider ?? (_searchProvider = ExamineManager.Instance.SearchProviderCollection["ExternalSearcher"]); }
        }

        protected override PagedResult<IPublishedContent> PerformSearch(QueryStructure query)
        {
            if (query.Lucene.IsNullOrWhiteSpace()) throw new HttpResponseException(HttpStatusCode.NotFound);

            //TODO: This would be more efficient if we went straight to the ExamineManager and used it's built in Skip method
            // but then we have to write our own model mappers and don't have time for that right now.

            var result = Umbraco.ContentQuery.TypedSearch(SearchProvider.CreateSearchCriteria().RawQuery(query.Lucene), SearchProvider)
                .ToArray();

            var paged = result.Skip(GetSkipSize(query.PageIndex, query.PageSize)).Take(query.PageSize);
            
            return new PagedResult<IPublishedContent>(result.Length, query.PageIndex + 1, query.PageSize)
            {
                Items = paged
            };
        }

        protected override IEnumerable<IPublishedContent> GetRootContent()
        {
            return Umbraco.TypedContentAtRoot();
        }

        protected override ContentMetadataRepresentation GetMetadataForItem(int id)
        {
            var found = Umbraco.TypedContent(id);
            if (found == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            var result = new ContentMetadataRepresentation(LinkTemplate, id)
            {
                Fields = GetDefaultFieldMetaData(),
                // NOTE: we cannot determine this from IPublishedContent
                Properties = null, 
                // NOTE: null because IPublishedContent is readonly
                CreateTemplate = null
            };
            return result;
        }

        protected override IPublishedContent GetItem(int id)
        {
            return Umbraco.TypedContent(id);
        }

        protected override PagedResult<IPublishedContent> GetChildContent(int id, long pageIndex = 0, int pageSize = 100)
        {
            var content = Umbraco.TypedContent(id);
            if (content == null) throw new HttpResponseException(HttpStatusCode.NotFound);
            var resolved = content.Children.ToArray();

            return new PagedResult<IPublishedContent>(resolved.Length, pageIndex + 1, pageSize)
            {
                Items = resolved.Skip(GetSkipSize(pageIndex, pageSize)).Take(pageSize)
            };
        }

        protected override PagedResult<IPublishedContent> GetDescendantContent(int id, long pageIndex = 0, int pageSize = 100)
        {
            var content = Umbraco.TypedContent(id);
            if (content == null) throw new HttpResponseException(HttpStatusCode.NotFound);
            var resolved = content.Descendants().ToArray();

            return new PagedResult<IPublishedContent>(resolved.Length, pageIndex + 1, pageSize)
            {
                Items = resolved.Skip(GetSkipSize(pageIndex, pageSize)).Take(pageSize)
            };
        }

        protected override IContentLinkTemplate<int> LinkTemplate
        {
            get { return new PublishedContentLinkTemplate(CurrentVersionRequest); }
        }

        /// <summary>
        /// Creates the content representation from the entity based on the current API version
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected override ContentRepresentation CreateRepresentation(IPublishedContent entity)
        {
            //create it with the current version link representation
            var representation = new ContentRepresentation(LinkTemplate);
            return Mapper.Map(entity, representation);
        }

    }
}