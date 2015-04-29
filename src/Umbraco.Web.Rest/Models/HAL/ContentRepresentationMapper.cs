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
                .ForMember(representation => representation.Properties, expression => expression.Ignore());
        }
    }
}