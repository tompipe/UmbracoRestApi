using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Umbraco.Core.Models;

namespace Umbraco.Web.Rest.Tests.TestHelpers
{
    public class ModelMocks
    {
        public static IContent SimpleMockedContent(int id = 123)
        {
            return Mock.Of<IContent>(
                content => content.Id == id
                           && content.Published == true
                           && content.CreateDate == DateTime.Now.AddDays(-2)
                           && content.CreatorId == 0
                           && content.HasIdentity == true
                           && content.ContentType == Mock.Of<IContentType>(ct => ct.Id == 99 && ct.Alias == "testType")
                           && content.ContentTypeId == 10
                           && content.Level == 1
                           && content.Name == "Home"
                           && content.Path == "-1,123"
                           && content.ParentId == 456
                           && content.SortOrder == 1
                           && content.Template == Mock.Of<ITemplate>(te => te.Id == 9 && te.Alias == "home")
                           && content.UpdateDate == DateTime.Now.AddDays(-1)
                           && content.WriterId == 1
                           && content.PropertyTypes == new List<PropertyType>(new[]
                           {
                               new PropertyType("testEditor", DataTypeDatabaseType.Nvarchar, "testProperty1") {Name = "Test Property1", Mandatory = true, ValidationRegExp = ""},
                               new PropertyType("testEditor", DataTypeDatabaseType.Nvarchar, "testProperty2") {Name = "Test Property2", Mandatory = false, ValidationRegExp = "zyxw"}
                           })
                           && content.Properties == new PropertyCollection(new[]
                           {
                               new Property(3, Guid.NewGuid(),
                                   new PropertyType("testEditor", DataTypeDatabaseType.Nvarchar, "testProperty1"), "property value1"),
                               new Property(3, Guid.NewGuid(),
                                   new PropertyType("testEditor", DataTypeDatabaseType.Nvarchar, "testProperty2"), "property value2"),
                           }));
        }
    }
}
