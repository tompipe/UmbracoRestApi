using System;
using Newtonsoft.Json;
using WebApi.Hal;

namespace Umbraco.Web.Rest.Models
{
    public class ValidationErrorRepresentation : Representation
    {
        public string Message { get; set; }
        public string LogRef { get; set; }

        public override string Rel
        {
            get { return "about"; }
            set { }
        }

        public override string Href
        {
            get { return "http://our.umbraco.org/documentation/"; }
            set { }
        }
    }
}