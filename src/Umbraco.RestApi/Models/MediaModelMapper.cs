using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;

namespace Umbraco.RestApi.Models
{
    public class MediaModelMapper : MapperConfiguration
    {

        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {

            config.CreateMap<IMedia, ContentRepresentation>()
                    .ForMember(representation => representation.HasChildren, expression => expression.MapFrom(content =>
                        applicationContext.Services.MediaService.HasChildren(content.Id)))
                    
                    .ForMember(representation => representation.Properties, expression => expression.ResolveUsing(content =>
                    {
                        var d = new Dictionary<string, object>();
                        foreach (var propertyType in content.PropertyTypes)
                        {
                            var prop = content.HasProperty(propertyType.Alias) ? content.Properties[propertyType.Alias] : null;
                            if (prop != null)
                            {
                                d.Add(propertyType.Alias, prop.Value);
                            }
                        }
                        return d;
                    }));

            config.CreateMap<IMedia, ContentTemplate>()
                .IgnoreAllUnmapped()
                .ForMember(representation => representation.Properties, expression => expression.ResolveUsing(content =>
                {
                    return content.PropertyTypes.ToDictionary<PropertyType, string, object>(propertyType => propertyType.Alias, propertyType => "");
                }));

            config.CreateMap<IMedia, IDictionary<string, ContentPropertyInfo>>()
                .ConstructUsing(content =>
                {
                    var result = new Dictionary<string, ContentPropertyInfo>();
                    foreach (var propertyType in content.PropertyTypes)
                    {
                        result[propertyType.Alias] = new ContentPropertyInfo
                        {
                            Label = propertyType.Name,
                            ValidationRegexExp = propertyType.ValidationRegExp,
                            ValidationRequired = propertyType.Mandatory
                        };
                    }
                    return result;
                });

            config.CreateMap<ContentRepresentation, IMedia>()
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
                            found.Value = propertyRepresentation.Value;
                        }
                    }
                });
        }
    }
}
