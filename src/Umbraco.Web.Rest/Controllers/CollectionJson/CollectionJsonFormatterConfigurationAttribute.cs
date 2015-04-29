using System;
using System.Web.Http.Controllers;
using CollectionJson.Client;

namespace Umbraco.Web.Rest.Controllers.CollectionJson
{
    /// <summary>
    /// Applying this attribute to any webapi controller will ensure that the CollectionJsonFormatter is added to the controller
    /// </summary>
    public class CollectionJsonFormatterConfigurationAttribute : Attribute, IControllerConfiguration
    {
        public virtual void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Formatters.Insert(0, new CollectionJsonFormatter());
        }
    }

}