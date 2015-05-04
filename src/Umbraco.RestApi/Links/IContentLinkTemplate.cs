using WebApi.Hal;

namespace Umbraco.RestApi.Links
{
    public interface IContentLinkTemplate
    {
        Link RootContent { get; }
        Link ContentItem { get; }
        Link ParentContent { get; }
        Link PagedDescendantContent { get; }
        Link PagedChildContent { get; }
        Link ContentMetaData { get; }
    }
}