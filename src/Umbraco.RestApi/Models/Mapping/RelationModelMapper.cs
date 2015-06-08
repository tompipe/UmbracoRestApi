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
    class RelationModelMapper : MapperConfiguration
    {

        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IRelation, RelationRepresentation>()
                .ForMember(representation => representation.RelationTypeAlias, expression => expression.MapFrom(member => member.RelationType.Alias));


            config.CreateMap<RelationRepresentation, IRelation>()
                .ConstructUsing((RelationRepresentation source) => new Relation(source.ParentId, source.ChildId, ApplicationContext.Current.Services.RelationService.GetRelationTypeByAlias(source.RelationTypeAlias)))

                .ForMember(dto => dto.ParentId, expression => expression.Ignore())
                .ForMember(dto => dto.ChildId, expression => expression.Ignore())

                .ForMember(dest => dest.Id, expression => expression.Condition(representation => (representation.Id > 0)));
        }
    }
}
