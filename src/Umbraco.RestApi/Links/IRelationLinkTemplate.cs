using System;
namespace Umbraco.RestApi.Links
{
    public interface IRelationLinkTemplate
    {
        WebApi.Hal.Link Children(int id);

        WebApi.Hal.Link ContentItem { get; }

        WebApi.Hal.Link Parents(int id);

        WebApi.Hal.Link RootContent { get; }
    }
}
