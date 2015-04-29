using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;

namespace Umbraco.Web.Rest.Models.OData
{
    public class ContentItemMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IContent, ContentItem>()
                .ForMember(content => content.Properties, expression => expression.ResolveUsing(content =>
                {
                    var d = new Dictionary<string, object>();
                    foreach (var propertyType in content.PropertyTypes)
                    {

                        var prop = content.HasProperty(propertyType.Alias) ? content.Properties[propertyType.Alias] : null;
                        if (prop != null)
                        {
                            d.Add(propertyType.Alias, new ContentItemProperty
                            {
                                Value = new Dictionary<string, object> { { "value", prop.Value } },                                
                                Label = propertyType.Name,
                                PropertyType = propertyType
                                //TODO: Validation
                            });
                        }                        
                    }
                    return new ContentItemProperties()
                    {
                        Properties = d
                    };
                }));
            
            config.CreateMap<ContentItem, IContent>()
                .IgnoreAllUnmapped()
                .ForMember(content => content.Name, expression => expression.MapFrom(item => item.Name))
                //TODO: IF people are 'updating' will this move it or cause problems?
                .ForMember(content => content.ParentId, expression => expression.MapFrom(item => item.ParentId))
                //TODO: IF people are 'updating' this will probably cause issues
                .ForMember(content => content.SortOrder, expression => expression.MapFrom(item => item.SortOrder))                
                //TODO: Need to be able to publish/unpublish - but this is only relavent for Content (not published content, media, members)
                // so would need to create a derived model for that maybe
                .AfterMap((item, content) =>
                {
                    //TODO: Set template

                    //TODO: Set properties
                });

        }
    }
}
