using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using CollectionJson;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Umbraco.Core.Services;
using Umbraco.Web.Rest.Controllers;
using Umbraco.Web.Rest.Serialization;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Rest.Tests.TestHelpers
{
    /// <summary>
    /// Custom activator to create instances of our controllers with an UmbracoContext
    /// </summary>
    public class CollectionJsonTestControllerActivator<TItem> : TestControllerActivatorBase
    {
        private readonly Func<HttpRequestMessage, UmbracoContext, ITypedPublishedContentQuery, IContentService, IMediaService, IMemberService, Tuple<ICollectionJsonDocumentWriter<TItem>, ICollectionJsonDocumentReader<TItem>>> _onServicesCreated;

        public CollectionJsonTestControllerActivator(Func<HttpRequestMessage, UmbracoContext, ITypedPublishedContentQuery, IContentService, IMediaService, IMemberService, Tuple<ICollectionJsonDocumentWriter<TItem>, ICollectionJsonDocumentReader<TItem>>> onServicesCreated)
        {
            _onServicesCreated = onServicesCreated;
        }

        protected override ApiController CreateController(Type controllerType, HttpRequestMessage msg, UmbracoHelper helper, ITypedPublishedContentQuery qry, IContentService contentService, IMediaService mediaService, IMemberService memberService)
        {
            var readerWriter = _onServicesCreated(msg, helper.UmbracoContext, qry, contentService, mediaService, memberService);

            //Create the controller with all dependencies
            var ctor = controllerType.GetConstructor(new[]
                {
                    typeof(UmbracoContext), 
                    typeof(UmbracoHelper), 
                    typeof(ICollectionJsonDocumentWriter<TItem>),
                    typeof(ICollectionJsonDocumentReader<TItem>)
                });

            if (ctor == null)
            {
                throw new MethodAccessException("Could not find the required constructor for the controller");
            }

            var created = (UmbracoApiControllerBase)ctor.Invoke(new object[]
                    {
                        //ctor args
                        helper.UmbracoContext, 
                        helper,
                        readerWriter.Item1,
                        readerWriter.Item2
                    });

            return created;
        }
    }


    //public class TestControllerSelector : DefaultHttpControllerSelector
    //{

    //    public TestControllerSelector(HttpConfiguration configuration)
    //        : base(configuration)
    //    {
    //    }

    //    /// <summary>
    //    /// Gets the name of the controller for the specified <see cref="T:System.Net.Http.HttpRequestMessage"/>.
    //    /// </summary>
    //    /// <returns>
    //    /// The name of the controller for the specified <see cref="T:System.Net.Http.HttpRequestMessage"/>.
    //    /// </returns>
    //    /// <param name="request">The HTTP request message.</param>
    //    public override string GetControllerName(HttpRequestMessage request)
    //    {
    //        var name = base.GetControllerName(request);
    //        return name;
    //    }

    //    /// <summary>
    //    /// Returns a map, keyed by controller string, of all <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor"/> that the selector can select. 
    //    /// </summary>
    //    /// <returns>
    //    /// A map of all <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor"/> that the selector can select, or null if the selector does not have a well-defined mapping of <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor"/>.
    //    /// </returns>
    //    public override IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
    //    {
    //        var mapping = base.GetControllerMapping();
    //        return mapping;
    //    }

    //    /// <summary>
    //    /// Selects a <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor"/> for the given <see cref="T:System.Net.Http.HttpRequestMessage"/>.
    //    /// </summary>
    //    /// <returns>
    //    /// The <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor"/> instance for the given <see cref="T:System.Net.Http.HttpRequestMessage"/>.
    //    /// </returns>
    //    /// <param name="request">The HTTP request message.</param>
    //    public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
    //    {
    //        IHttpRouteData routeData = request.GetRouteData();
    //        HttpControllerDescriptor controllerDescriptor;
    //        if (routeData != null)
    //        {
    //            //controllerDescriptor = GetDirectRouteController(routeData);
    //            //if (controllerDescriptor != null)
    //            //{
    //            //    return controllerDescriptor;
    //            //}
    //        }

    //        string controllerName = GetControllerName(request);
    //        if (String.IsNullOrEmpty(controllerName))
    //        {
    //            //throw new HttpResponseException(request.CreateErrorResponse(
    //            //    HttpStatusCode.NotFound,
    //            //    Error.Format(SRResources.ResourceNotFound, request.RequestUri),
    //            //    Error.Format(SRResources.ControllerNameNotFound, request.RequestUri)));
    //        }

    //        var found = base.SelectController(request);
    //        return found;
    //    }
    //}

    //public class TestControllerTypeResolver : DefaultHttpControllerTypeResolver
    //{

    //    public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
    //    {
    //        var types = base.GetControllerTypes(assembliesResolver);
    //        return types;
    //    }
    //}
}