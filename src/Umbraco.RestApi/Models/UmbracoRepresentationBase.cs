using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Umbraco.Core.Models;
using Umbraco.RestApi.Serialization;
using WebApi.Hal;

namespace Umbraco.RestApi.Models
{
    public class UmbracoRepresentationBase : Representation
    {
        public UmbracoRepresentationBase()
        {
            Properties = new Dictionary<string, object>();
        }

        public int Id { get; set; }
        public Guid Key { get; set; }

        [Required]
        [Display(Name = "name")]
        public string Name { get; set; }

        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset UpdateDate { get; set; }
        public PublishedItemType ItemType { get; set; }

        [JsonConverter(typeof(ExplicitlyCasedDictionaryKeyJsonConverter<object>))]
        public IDictionary<string, object> Properties { get; set; }
    }
}