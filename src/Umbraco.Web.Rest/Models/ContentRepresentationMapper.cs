using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Rest.Models
{
    public class ContentRepresentationMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IContent, ContentRepresentation>()
                .ForMember(representation => representation.HasChildren, expression => expression.MapFrom(content => 
                    applicationContext.Services.ContentService.HasChildren(content.Id)))
                .ForMember(representation => representation.Properties, expression => expression.ResolveUsing(content =>
                {
                    var d = new Dictionary<string, ContentPropertyRepresentation>();
                    foreach (var propertyType in content.PropertyTypes)
                    {
                        var prop = content.HasProperty(propertyType.Alias) ? content.Properties[propertyType.Alias] : null;
                        if (prop != null)
                        {
                            d.Add(propertyType.Alias, new ContentPropertyRepresentation
                            {
                                Value = prop.Value,
                                Label = propertyType.Name,
                                //TODO: Validation
                            });
                        }
                    }
                    return d;
                }));

            config.CreateMap<ContentRepresentation, IContent>()
                .IgnoreAllUnmapped()
                .ForMember(content => content.Name, expression => expression.MapFrom(representation => representation.Name))
                //TODO: This could be used to 'Move' an item but we'd have to deal with that, not sure we should deal with that in a mapping
                //.ForMember(content => content.ParentId, expression => expression.MapFrom(representation => representation.ParentId))
                //TODO: This could be used to 'Sort' an item but we'd have to deal with that, not sure we should deal with that in a mapping
                //.ForMember(content => content.SortOrder, expression => expression.MapFrom(representation => representation.SortOrder))
                .AfterMap((representation, content) =>
                {
                    //TODO: Map template;
                    
                    foreach (var propertyRepresentation in representation.Properties)
                    {
                        var found = content.HasProperty(propertyRepresentation.Key) ? content.Properties[propertyRepresentation.Key] : null;
                        if (found != null)
                        {
                            found.Value = propertyRepresentation.Value.Value;
                        }
                    }
                });

            config.CreateMap<IPublishedContent, ContentRepresentation>()
                .ForMember(representation => representation.HasChildren, expression => expression.MapFrom(content => content.Children.Any()))
                .ForMember(representation => representation.Properties, expression => expression.ResolveUsing(content =>
                {
                    return content.Properties.ToDictionary(property => property.PropertyTypeAlias,
                        property => new ContentPropertyRepresentation
                        {
                            Value = property.Value
                        });
                }));
        }
    }
}