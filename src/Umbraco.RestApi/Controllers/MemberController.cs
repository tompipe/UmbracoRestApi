using System;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.RestApi.Links;
using Umbraco.RestApi.Models;
using Umbraco.RestApi.Routing;
using Umbraco.Web;

namespace Umbraco.RestApi.Controllers
{
    [UmbracoRoutePrefix("rest/v1/members")]
    public class MemberController : UmbracoHalController<int, IMember, MemberRepresentation, ILinkTemplate>
    {
        public MemberController()
        {
        }

        public MemberController(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper)
            : base(umbracoContext, umbracoHelper)
        {
        }

        protected override ContentMetadataRepresentation GetMetadataForItem(int id)
        {
            throw new NotImplementedException();
        }

        protected override IMember GetItem(int id)
        {
            throw new NotImplementedException();
        }

        protected override ILinkTemplate LinkTemplate
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Creates the content representation from the entity based on the current API version
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected override MemberRepresentation CreateRepresentation(IMember entity)
        {
            //create it with the current version link representation
            var representation = new MemberRepresentation(LinkTemplate);
            return Mapper.Map(entity, representation);
        }
    }
}