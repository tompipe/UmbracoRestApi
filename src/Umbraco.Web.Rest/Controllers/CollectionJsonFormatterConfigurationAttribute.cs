using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using CollectionJson;
using CollectionJson.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Umbraco.Web.Rest.Controllers
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

    //public class CollectionJsonContractResolver2 : CamelCasePropertyNamesContractResolver
    //{
    //    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    //    {
    //        var property = base.CreateProperty(member, memberSerialization);
    //        if (property.DeclaringType.Namespace == "CollectionJson")
    //        {
    //            property.ShouldSerialize =
    //                instance =>
    //                {
    //                    var val = property.ValueProvider.GetValue(instance);
    //                    var list = val as IList;
    //                    if (list != null)
    //                    {
    //                        return list.Count > 0;
    //                    }
    //                    return true;
    //                };
    //        }
    //        return property;
    //    }

    //}

    //public class CollectionJsonFormatter2 : JsonMediaTypeFormatter
    //{
    //    public CollectionJsonFormatter2()
    //    {
    //        SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue(Collection.MediaType));
    //        SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    //        SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
    //        SerializerSettings.ContractResolver =
    //                    new CollectionJsonContractResolver2();
    //    }

    //    public override bool CanWriteType(Type type)
    //    {

    //        return base.CanWriteType(type) && (typeof(IReadDocument).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()) ||
    //            typeof(IWriteDocument).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()));
    //    }

    //    public override bool CanReadType(Type type)
    //    {
    //        var readable = base.CanReadType(type) && (typeof(IReadDocument).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()) ||
    //            typeof(IWriteDocument).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()));
    //        return readable;
    //    }

    //    public override System.Threading.Tasks.Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, System.Net.TransportContext transportContext)
    //    {
    //        if (typeof(IReadDocument).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()) && type != typeof(ReadDocument))
    //        {
    //            return base.WriteToStreamAsync(typeof(IReadDocument), new ReadDocumentDecorator((IReadDocument)value), writeStream, content, transportContext);
    //        }

    //        if (typeof(IWriteDocument).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()) && type != typeof(WriteDocument))
    //        {
    //            return base.WriteToStreamAsync(typeof(IWriteDocument), new WriteDocumentDecorator((IWriteDocument)value), writeStream, content, transportContext);
    //        }

    //        return base.WriteToStreamAsync(type, value, writeStream, content, transportContext);
    //    }


    //    public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
    //    {
    //        if (typeof(IWriteDocument).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()) &
    //            type != typeof(WriteDocument))
    //        {
    //            return base.ReadFromStreamAsync(typeof(WriteDocument), readStream, content, formatterLogger);
    //        }
    //        return base.ReadFromStreamAsync(type, readStream, content, formatterLogger);
    //    }

    //    private class ReadDocumentDecorator : IReadDocument
    //    {
    //        private readonly IReadDocument _innerReadDocument;

    //        public ReadDocumentDecorator(IReadDocument innerReadDocument)
    //        {
    //            _innerReadDocument = innerReadDocument;
    //        }

    //        public Collection Collection
    //        {
    //            get { return _innerReadDocument.Collection; }
    //        }
    //    }

    //    private class WriteDocumentDecorator : IWriteDocument
    //    {
    //        private readonly IWriteDocument _innerWriteDocument;

    //        public WriteDocumentDecorator(IWriteDocument innerWriteDocument)
    //        {
    //            _innerWriteDocument = innerWriteDocument;
    //        }

    //        public Template Template
    //        {
    //            get { return _innerWriteDocument.Template; }
    //        }
    //    }

    //}
}