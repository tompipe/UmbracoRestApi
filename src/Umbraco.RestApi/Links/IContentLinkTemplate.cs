using WebApi.Hal;

namespace Umbraco.RestApi.Links
{
    public interface IContentLinkTemplate : ILinkTemplate
    {        
        Link Parent { get; }
        Link PagedDescendants { get; }
        Link PagedChildren { get; }        
    }
}