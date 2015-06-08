using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.RestApi.Links;
using Umbraco.RestApi.Models;
using Umbraco.RestApi.Routing;
using Umbraco.Web;
using WebApi.Hal;

namespace Umbraco.RestApi.Controllers
{
    [UmbracoRoutePrefix("rest/v1/relations")]
    public class RelationsController : UmbracoHalControllerBase
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public RelationsController()
        {

        }

        /// <summary>
        /// All dependencies
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="umbracoHelper"></param>
        /// <param name="searchProvider"></param>
        public RelationsController(
            UmbracoContext umbracoContext,
            UmbracoHelper umbracoHelper)
            : base(umbracoContext, umbracoHelper)
        { }

        //QUERY RELATIONS
        [HttpGet]
        [CustomRoute("children/{id}")]
        public HttpResponseMessage GetByParent(int id, string relationType = null)
        {
            var parent = EntityService.Get(id);
            if (parent == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var relations = (string.IsNullOrEmpty(relationType)) ? RelationService.GetByParent(parent) : RelationService.GetByParent(parent, relationType);
            var relationsRep = new RelationListRepresentation(relations.Select(CreateRepresentation).ToList(), new RelationLinkTemplate(CurrentVersionRequest));
            return Request.CreateResponse(HttpStatusCode.OK, relationsRep);
        }

        [HttpGet]
        [CustomRoute("parents/{id}")]
        public HttpResponseMessage GetByChild(int id, string relationType = null)
        {
            var child = EntityService.Get(id);
            if (child == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);


            var relations = (string.IsNullOrEmpty(relationType)) ? RelationService.GetByChild(child) : RelationService.GetByChild(child, relationType);
            var relationsRep = new RelationListRepresentation(relations.Select(CreateRepresentation).ToList(), new RelationLinkTemplate(CurrentVersionRequest));
            return Request.CreateResponse(HttpStatusCode.OK, relationsRep);
        }



        //RELATIONS CRUD
        [HttpGet]
        [CustomRoute("{id}")]
        public HttpResponseMessage Get(int id)
        {
            var result = RelationService.GetById(id);

            return result == null
                ? Request.CreateResponse(HttpStatusCode.NotFound)
                : Request.CreateResponse(HttpStatusCode.OK, CreateRepresentation(result));
        }

        [HttpPost]
        [CustomRoute("")]
        public HttpResponseMessage Post(RelationRepresentation representation)
        {
            try
            {
                var relation = Mapper.Map<IRelation>(representation);
                RelationService.Save(relation);
                return Request.CreateResponse(HttpStatusCode.OK, CreateRepresentation(relation));
            }
            catch (ModelValidationException exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Errors);
            }
        }

        [HttpPut]
        [CustomRoute("{id}")]
        public HttpResponseMessage Put(int id, RelationRepresentation rel)
        {
            try
            {
                var found = RelationService.GetById(id);
                if (found == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                Mapper.Map(rel, found);
                RelationService.Save(found);

                return Request.CreateResponse(HttpStatusCode.OK, CreateRepresentation(found));
            }
            catch (ModelValidationException exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Errors);
            }
        }

        [HttpDelete]
        [CustomRoute("{id}")]
        public virtual HttpResponseMessage Delete(int id)
        {
            var found = RelationService.GetById(id);
            if (found == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            RelationService.Delete(found);
            return Request.CreateResponse(HttpStatusCode.OK);
        }



        private RelationRepresentation CreateRepresentation(IRelation relation)
        {
            var parentLinkTemplate = GetLinkTemplate(relation.RelationType.ParentObjectType);
            var childLinkTemplate = GetLinkTemplate(relation.RelationType.ChildObjectType);

            var rep = new RelationRepresentation(RelationLinkTemplate, parentLinkTemplate, childLinkTemplate);
            return Mapper.Map(relation, rep);
        }

        private IContentLinkTemplate GetLinkTemplate(Guid nodeObjectType)
        {
            switch (nodeObjectType.ToString().ToUpper())
            {
                case Core.Constants.ObjectTypes.ContentItem:
                    return new ContentLinkTemplate(CurrentVersionRequest);
                case Core.Constants.ObjectTypes.Media:
                    return new MediaLinkTemplate(CurrentVersionRequest);
                case Core.Constants.ObjectTypes.Member:
                    return new MembersLinkTemplate(CurrentVersionRequest);
                default:
                    break;
            }

            return null;
        }

        private IRelationLinkTemplate RelationLinkTemplate
        {
            get { return new RelationLinkTemplate(CurrentVersionRequest); }
        }

        protected IRelationService RelationService
        {
            get { return ApplicationContext.Services.RelationService; }
        }

        protected IEntityService EntityService
        {
            get { return ApplicationContext.Services.EntityService; }
        }
    }


}
