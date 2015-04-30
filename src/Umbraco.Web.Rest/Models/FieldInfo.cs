namespace Umbraco.Web.Rest.Models
{
    public class FieldInfo
    {
        public string Label { get; set; }
        
        public bool ValidationRequired { get; set; }
        public string ValidationRegexExp { get; set; }
    }
}