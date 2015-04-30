using WebApi.Hal;

namespace Umbraco.Web.Rest.Links
{
    public interface IContentLinkTemplate
    {
        Link RootContent { get; }
        Link ContentItem { get; }
        Link ParentContent { get; }
        Link ChildContent { get; }
        Link ContentMetaData { get; }
    }
}