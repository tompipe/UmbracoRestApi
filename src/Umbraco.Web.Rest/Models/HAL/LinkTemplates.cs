using WebApi.Hal;

namespace Umbraco.Web.Rest.Models.HAL
{
    public static class LinkTemplates
    {
        public static class Content
        {
            public static Link RootContent
            {
                get { return new Link("root", "~/content"); }
            }

            public static Link ContentItem
            {
                get { return new Link("content", "~/content/{id}"); }
            }

            public static Link ParentContent
            {
                get { return new Link("parent", "~/content/{parentId}"); }
            }

            public static Link ChildContent
            {
                get { return new Link("children", "~/content/{id}/children"); }
            }
        }

    }
}