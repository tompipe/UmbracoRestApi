using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;

namespace Umbraco.Web.Rest.Models
{
    /// <summary>
    /// Helper class to build the EdmModels by either explicit or implicit method.
    /// </summary>
    public class UmbracoContentModelBuilder
    {
        private readonly HttpConfiguration _config;

        public UmbracoContentModelBuilder(HttpConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Get the EdmModel.
        /// </summary>
        /// <returns></returns>
        public IEdmModel GetEdmModel()
        {
            return GetImplicitEdmModel();            
        }

        /// <summary>
        /// Generates a model from a few seeds (i.e. the names and types of the entity sets)
        /// by applying conventions.
        /// </summary>
        /// <returns>An implicitly configured model</returns>    
        private IEdmModel GetImplicitEdmModel()
        {
            var builder = new ODataConventionModelBuilder(_config);

            //do declarations

            

            builder.EntitySet<ContentItem>("Content");
            builder.ComplexType<ContentItemProperty>()
                .Ignore(property => property.PropertyType);
            
            //when it's created, customize a few things:
            builder.OnModelCreating = mb =>
            {
                //get the content entity set to modify it
                var content = mb.EntitySet<ContentItem>("Content");
                
                //This will create a custom navigation link to the parent with it's id instead of the default:
                //      /umbraco/rest/v1/odata/Content(456)
                // This is the default:
                //      /umbraco/rest/v1/odata/Content(123)/Parent"

                content.HasNavigationPropertyLink(content.EntityType.NavigationProperties.Single(x => x.Name == "Parent"),
                    (context, navProperty) =>
                    {
                        object parentId;
                        if (context.EdmObject.TryGetPropertyValue("ParentId", out parentId))
                        {
                            return new Uri(context.Url.CreateODataLink(
                                new EntitySetPathSegment("Content"),
                                new KeyValuePathSegment(parentId.ToString())));    
                        }
                        return null;
                    },
                    //By specifying false, this will ensure that the link it output with JSON "light" format, 
                    // otherwise it's only available with the verbose format.
                    followsConventions: false);

                //This will override a navigation link to the children, we do this so it shows up with JSON "light" format:
                //      /umbraco/rest/v1/odata/Content(123)/Children"

                content.HasNavigationPropertyLink(content.EntityType.NavigationProperties.Single(x => x.Name == "Children"),
                    (context, navProperty) =>
                    {
                        object id;
                        if (context.EdmObject.TryGetPropertyValue("Id", out id))
                        {
                            return new Uri(context.Url.CreateODataLink(
                            new EntitySetPathSegment("Content"),
                            new KeyValuePathSegment(id.ToString()),
                            new NavigationPathSegment(navProperty.Name)));
                        }
                        return null;
                    },
                    //By specifying false, this will ensure that the link it output with JSON "light" format
                    // otherwise it's only available with the verbose format.
                    followsConventions: false);
            };            
            
            //camel case all the things, 
            // TODO: We'll see what repercussions this has, we don't really want this for property names!!! 
            // otherwise this might be of interest: https://aspnet.codeplex.com/SourceControl/latest#Samples/WebApi/OData/v4/ODataModelAliasingSample/Readme.txt
            builder.EnableLowerCamelCase();

            return builder.GetEdmModel();
        }
    }
}