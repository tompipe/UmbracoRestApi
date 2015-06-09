using WebApi.Hal;

namespace Umbraco.RestApi.Links
{
    public interface IContentLinkTemplate<in TId> : ILinkTemplate
        where TId: struct 
    {        
        Link Parent { get; }
        Link PagedDescendants(TId id);
        Link PagedChildren(TId id);

    }
}