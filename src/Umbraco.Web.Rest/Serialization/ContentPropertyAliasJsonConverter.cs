using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.Web.Rest.Models;

namespace Umbraco.Web.Rest.Serialization
{
    /// <summary>
    /// Custom converter to ensure that property aliases don't get camelcased
    /// </summary>
    public class ContentPropertyAliasJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IDictionary<string, object>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<IDictionary<string, object>>(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            foreach (var property in (IDictionary<string, object>)value)
            {
                writer.WritePropertyName(property.Key);
                serializer.Serialize(writer, property.Value);
            }

            writer.WriteEndObject();
        }
    }
}