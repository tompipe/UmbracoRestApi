using System;
namespace Umbraco.RestApi.Links
{
    public interface IRelationLinkTemplate
    {
        WebApi.Hal.Link Children { get; }
        WebApi.Hal.Link ContentItem { get; }
        WebApi.Hal.Link Parents { get; }
        WebApi.Hal.Link RootContent { get; }
    }
}
