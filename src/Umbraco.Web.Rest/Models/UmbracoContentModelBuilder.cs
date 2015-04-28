using System;
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
            // build the model by convention
            return GetImplicitEdmModel();
            // or build the model by hand
            //return GetExplicitEdmModel();
        }

        /// <summary>
        /// Generates a model explicitly/manually.
        /// </summary>
        /// <returns></returns>
        public IEdmModel GetExplicitEdmModel()
        {
            var modelBuilder = new ODataModelBuilder();

            const string contentEntityName = "Content";

            var content = modelBuilder.EntitySet<GenericContent>(contentEntityName);

            content.HasIdLink(
                entityContext =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("Id", out id);

                    return new Uri(entityContext.Url.CreateODataLink(
                        new EntitySetPathSegment(contentEntityName),
                        new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(id, ODataVersion.V4))));
                },
                followsConventions: true);

            content.HasEditLink(
                entityContext =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("Id", out id);
                    return new Uri(entityContext.Url.CreateODataLink(
                        new EntitySetPathSegment(contentEntityName),
                        new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(id, ODataVersion.V4))));
                },
                followsConventions: true);

            var nav = content.HasManyBinding(pf => pf.Children, content);

            //content.HasNavigationPropertyLink(new NavigationPropertyConfiguration(content.EntityType.HasMany()), 

            //var suppliers = modelBuilder.EntitySet<Supplier>("Suppliers");

            //suppliers.HasIdLink(
            //    entityContext =>
            //    {
            //        object id;
            //        entityContext.EdmObject.TryGetPropertyValue("ID", out id);
            //        return entityContext.Url.CreateODataLink(
            //            new EntitySetPathSegment(entityContext.EntitySet.Name),
            //            new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(id, ODataVersion.V4)));
            //    },
            //    followsConventions: true);

            //suppliers.HasEditLink(
            //    entityContext =>
            //    {
            //        object id;
            //        entityContext.EdmObject.TryGetPropertyValue("ID", out id);
            //        return entityContext.Url.CreateODataLink(
            //            new EntitySetPathSegment(entityContext.EntitySet.Name),
            //            new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(id, ODataVersion.V3)));
            //    },
            //    followsConventions: true);

            //var families = modelBuilder.EntitySet<ProductFamily>("ProductFamilies");

            //families.HasIdLink(
            //    entityContext =>
            //    {
            //        object id;
            //        entityContext.EdmObject.TryGetPropertyValue("ID", out id);
            //        return entityContext.Url.CreateODataLink(
            //            new EntitySetPathSegment(entityContext.EntitySet.Name),
            //            new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(id, ODataVersion.V3)));
            //    },
            //    followsConventions: true);

            //families.HasEditLink(
            //    entityContext =>
            //    {
            //        object id;
            //        entityContext.EdmObject.TryGetPropertyValue("ID", out id);
            //        return entityContext.Url.CreateODataLink(
            //            new EntitySetPathSegment(entityContext.EntitySet.Name),
            //            new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(id, ODataVersion.V3)));
            //    },
            //    followsConventions: true);

            var contentType = content.EntityType;

            contentType.HasKey(p => p.Id);
            contentType.Property(p => p.Name);
            //contentType.Property(p => p.ReleaseDate);
            //contentType.Property(p => p.SupportedUntil);

            //modelBuilder.Entity<RatedProduct>().DerivesFrom<Product>().Property(rp => rp.Rating);

            //var address = modelBuilder.ComplexType<Address>();
            //address.Property(a => a.City);
            //address.Property(a => a.Country);
            //address.Property(a => a.State);
            //address.Property(a => a.Street);
            //address.Property(a => a.ZipCode);

            //var supplier = suppliers.EntityType;
            //supplier.HasKey(s => s.ID);
            //supplier.Property(s => s.Name);
            //supplier.ComplexProperty(s => s.Address);

            //var productFamily = families.EntityType;
            //productFamily.HasKey(pf => pf.ID);
            //productFamily.Property(pf => pf.Name);
            //productFamily.Property(pf => pf.Description);

            //// Create relationships and bindings in one go
            //content.HasRequiredBinding(p => p.Family, families);
            //families.HasManyBinding(pf => pf.Products, content);
            //families.HasOptionalBinding(pf => pf.Supplier, suppliers);
            //suppliers.HasManyBinding(s => s.ProductFamilies, families);

            //// Create navigation Link builders
            //content.HasNavigationPropertiesLink(
            //    contentType.NavigationProperties,
            //    (entityContext, navigationProperty) =>
            //    {
            //        object id;
            //        entityContext.EdmObject.TryGetPropertyValue("ID", out id);
            //        return new Uri(entityContext.Url.CreateODataLink(
            //            new EntitySetPathSegment(entityContext.EntitySet.Name),
            //            new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(id, ODataVersion.V3)),
            //            new NavigationPathSegment(navigationProperty.Name)));
            //    },
            //    followsConventions: true);

            //families.HasNavigationPropertiesLink(
            //    productFamily.NavigationProperties,
            //    (entityContext, navigationProperty) =>
            //    {
            //        object id;
            //        entityContext.EdmObject.TryGetPropertyValue("ID", out id);
            //        return new Uri(entityContext.Url.CreateODataLink(
            //            new EntitySetPathSegment(entityContext.EntitySet.Name),
            //            new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(id, ODataVersion.V3)),
            //            new NavigationPathSegment(navigationProperty.Name)));
            //    },
            //    followsConventions: true);

            //suppliers.HasNavigationPropertiesLink(
            //    supplier.NavigationProperties,
            //    (entityContext, navigationProperty) =>
            //    {
            //        object id;
            //        entityContext.EdmObject.TryGetPropertyValue("ID", out id);
            //        return new Uri(entityContext.Url.CreateODataLink(
            //            new EntitySetPathSegment(entityContext.EntitySet.Name),
            //            new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(id, ODataVersion.V3)),
            //            new NavigationPathSegment(navigationProperty.Name)));
            //    },
            //    followsConventions: true);

            //ActionConfiguration createProduct = productFamily.Action("CreateProduct");
            //createProduct.Parameter<string>("Name");
            //createProduct.Returns<int>();

            return modelBuilder.GetEdmModel();
        }

        /// <summary>
        /// Generates a model from a few seeds (i.e. the names and types of the entity sets)
        /// by applying conventions.
        /// </summary>
        /// <returns>An implicitly configured model</returns>    
        public IEdmModel GetImplicitEdmModel()
        {
            var modelBuilder = new ODataConventionModelBuilder(_config);
            var content = modelBuilder.EntitySet<GenericContent>("Content");

            //when it's created, customize a few things:
            modelBuilder.OnModelCreating = mb =>
            {
                //This will create a custom navigation link to the parent with it's id instead of the default:
                //      /umbraco/rest/v1/odata/Content(456)
                // This is the default:
                //      /umbraco/rest/v1/odata/Content(123)/Parent"

                content.HasNavigationPropertyLink(content.EntityType.NavigationProperties.Single(x => x.Name == "Parent"),
                    (context, property) => new Uri(context.Url.CreateODataLink(
                        new EntitySetPathSegment("Content"), new KeyValuePathSegment(context.EntityInstance.ParentId.ToString()))),
                    //By specifying false, this will ensure that the link it output with JSON "light" format, otherwise it's only available
                    // with the verbose format.
                    followsConventions: false);

                //content.HasNavigationPropertiesLink(
                //    content.EntityType.NavigationProperties,
                //    (entityContext, navigationProperty) =>
                //        new Uri(entityContext.Url.CreateODataLink(
                //            new EntitySetPathSegment("Content"),
                //            new KeyValuePathSegment(entityContext.EntityInstance.Id.ToString()),
                //            new NavigationPathSegment(navigationProperty.Name))),
                //    false);
            };            

            //content.HasNavigationPropertyLink(new NavigationPropertyConfiguration(content.EntityType.HasMany()), 
            
                //(context, navigation) =>
            //{
            //    return new Uri(context.Url.CreateODataLink(new EntitySetPathSegment("Content"), new KeyValuePathSegment(context.EntityInstance.ParentId.ToString())));
            //}, followsConventions: false);

            //content.HasEditLink(
            //   entityContext =>
            //   {
            //       object id;
            //       entityContext.EdmObject.TryGetPropertyValue("Id", out id);
            //       return new Uri(entityContext.Url.CreateODataLink(
            //           new EntitySetPathSegment("Content"),
            //           new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(id, ODataVersion.V4))));
            //   },
            //   followsConventions: true);

            //content.HasIdLink(
            //    entityContext =>
            //    {
            //        object id;
            //        entityContext.EdmObject.TryGetPropertyValue("Id", out id);

            //        return new Uri(entityContext.Url.CreateODataLink(
            //            new EntitySetPathSegment("Content"),
            //            new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(id, ODataVersion.V4))));
            //    },
            //    followsConventions: true);

            //modelBuilder.Entity<RatedProduct>().DerivesFrom<Product>();
            //modelBuilder.EntitySet<ProductFamily>("ProductFamilies");
            //modelBuilder.EntitySet<Supplier>("Suppliers");

            //ActionConfiguration createProduct = modelBuilder.Entity<ProductFamily>().Action("CreateProduct");
            //createProduct.Parameter<string>("Name");
            //createProduct.Returns<int>();

            return modelBuilder.GetEdmModel();
        }
    }
}