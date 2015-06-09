using System;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.RestApi.Links;
using Umbraco.RestApi.Models;
using Umbraco.RestApi.Routing;
using Umbraco.Web;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core.Services;
using System.Net;
using Umbraco.Core;
using System.Linq;
using Examine;
using Examine.Providers;

namespace Umbraco.RestApi.Controllers
{
    [UmbracoRoutePrefix("rest/v1/members")]
    public class MemberController : UmbracoHalController<int, IMember, MemberRepresentation, ILinkTemplate>
    {
        public MemberController()
        {
        }

        public MemberController(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper, BaseSearchProvider searchProvider)
            : base(umbracoContext, umbracoHelper)
        {
            if (searchProvider == null) throw new ArgumentNullException("searchProvider");
            _searchProvider = searchProvider;
        }

        private BaseSearchProvider _searchProvider;
        protected BaseSearchProvider SearchProvider
        {
            get { return _searchProvider ?? (_searchProvider = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"]); }
        }

        //TODO: Remove this
        [HttpPost]
        [CustomRoute("login")]
        public HttpResponseMessage Login(MemberLogin login)
        {
            try
            {
                if (Members.Login(login.Username, login.Password))
                {
                    // TODO: There must be a better way ?
                    var member = MemberService.GetByUsername(login.Username);
                    var rep = CreateRepresentation(member);

                    return Request.CreateResponse(HttpStatusCode.OK, rep);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }
            catch (ModelValidationException exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Errors);
            }
        }

        [HttpGet]
        [CustomRoute("")]
        public HttpResponseMessage Get(long pageIndex = 0, int pageSize = 100, string orderBy = "Name", string direction = "Ascending", string memberTypeAlias = null, string filter = "")
        {
            long totalRecords = 0;
            var direction_enum = Enum<Core.Persistence.DatabaseModelDefinitions.Direction>.Parse(direction);
            var members = MemberService.GetAll(pageIndex, pageSize, out totalRecords, orderBy, direction_enum, memberTypeAlias, filter);
            int totalPages = ((int)totalRecords + pageSize - 1) / pageSize;

            var representation = new PagedContentListRepresentation<MemberRepresentation>(
                members.Select(CreateRepresentation).ToList(),
                totalRecords,
                totalPages,
                pageIndex,
                pageSize,
                LinkTemplate,
                LinkTemplate.Root,
                new { });

            return Request.CreateResponse(HttpStatusCode.OK, representation);
        }

        //Don't route here (which is normally the default)
        [NonAction]
        public override HttpResponseMessage Get()
        {
            throw new NotImplementedException();
        }

        //BASIC CRUD
        protected override IMember CreateNew(MemberRepresentation content)
        {
            //we cannot continue here if the mandatory items are empty (i.e. name, etc...)
            if (!ModelState.IsValid)
            {
                throw ValidationException(ModelState, content);
            }


            var memberType = Services.MemberTypeService.Get(content.ContentTypeAlias);
            if (memberType == null)
            {
                ModelState.AddModelError("content.memberType", "No member type found with alias " + content.ContentTypeAlias);
                throw ValidationException(ModelState, content);
            }

            //create an item before persisting of the correct content type
            var created = MemberService.CreateMember(content.UserName, content.Email, content.Name, memberType);

            //Validate properties
            var validator = new ContentPropertyValidator<IMember>(ModelState, Services.DataTypeService);
            validator.ValidateItem(content, created);

            if (!ModelState.IsValid)
            {
                throw ValidationException(ModelState, content);
            }

            Mapper.Map(content, created);
            MemberService.Save(created);

            return created;
        }

        protected override IMember Update(int id, MemberRepresentation content)
        {
            var found = MemberService.GetById(id);
            if (found == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            //Validate properties
            var validator = new ContentPropertyValidator<IMember>(ModelState, Services.DataTypeService);
            validator.ValidateItem(content, found);

            if (!ModelState.IsValid)
            {
                throw ValidationException(ModelState, content, id: id);
            }

            Mapper.Map(content, found);
            MemberService.Save(found);

            return found;
        }

        protected override IMember SetFileOnProperty(int id, string property, System.Web.HttpPostedFileBase file)
        {
            var node = MemberService.GetById(id);
            if (node == null)
                return null;

            node.SetValue(property, file);
            MemberService.Save(node);

            return node;
        }

        public override HttpResponseMessage Delete(int id)
        {
            var member = MemberService.GetById(id);
            if (member == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            MemberService.Delete(member);
            return Request.CreateResponse(HttpStatusCode.OK);
        }




        protected override ContentMetadataRepresentation GetMetadataForItem(int id)
        {
            throw new NotImplementedException();
        }

        protected override IMember GetItem(int id)
        {
            return Services.MemberService.GetById(id);  
        }

        protected override ILinkTemplate LinkTemplate
        {
            get { return new MembersLinkTemplate(CurrentVersionRequest); }
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

        protected IMemberService MemberService
        {
            get { return ApplicationContext.Services.MemberService; }
        }
    }
}