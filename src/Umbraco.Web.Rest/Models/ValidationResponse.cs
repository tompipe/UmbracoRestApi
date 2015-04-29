using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Umbraco.Web.Rest.Serialization;

namespace Umbraco.Web.Rest.Models
{
    /// <summary>
    /// A validation error response
    /// </summary>
    /// <remarks>
    /// taken from http://soabits.blogspot.dk/2013/05/error-handling-considerations-and-best.html
    /// </remarks>
    public class ValidationResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("details")]
        public string Details { get; set; }
        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }
        [JsonProperty("httpStatusCode")]
        public int HttpStatusCode { get; set; }
        [JsonProperty("time")]
        public DateTimeOffset Time { get; set; }

        [JsonProperty("errors")]
        [JsonConverter((typeof(ErrorMessageJsonConverter)))]
        public IEnumerable<ValidationError> Errors { get; set; }

        [JsonProperty("additional")]
        public IDictionary<string, object> Additional { get; set; } 

    }
}
