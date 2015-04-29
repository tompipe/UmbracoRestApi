using Newtonsoft.Json;

namespace Umbraco.Web.Rest.Models
{
    public class ValidationError
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("field")]
        public string Field { get; set; }
    }
}