using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;

namespace Umbraco.Web.Rest.Models.HAL
{
    public class ContentRepresentationMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IEnumerable<IContent>, ContentListRepresentation>()
                .ConstructUsing((IEnumerable<IContent> contents) =>
                    new ContentListRepresentation(
                        new List<ContentRepresentation>(Mapper.Map<IEnumerable<ContentRepresentation>>(contents))));


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
        }
    }
}