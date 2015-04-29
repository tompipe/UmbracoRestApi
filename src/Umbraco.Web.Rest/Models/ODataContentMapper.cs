using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;

namespace Umbraco.Web.Rest.Models
{
    public class ODataContentMapper : MapperConfiguration
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
                                Label = propertyType.Name
                                //TODO: Validation
                            });
                        }                        
                    }
                    return new ContentItemProperties()
                    {
                        Properties = d
                    };
                }));

        }
    }
}
