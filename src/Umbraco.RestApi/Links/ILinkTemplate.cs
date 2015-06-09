using WebApi.Hal;

namespace Umbraco.RestApi.Links
{
    public interface ILinkTemplate
    {
        Link Root { get; }        
        Link Self { get; }
        Link MetaData { get; }
        Link Search { get; }
        Link Upload { get; }
    }
}