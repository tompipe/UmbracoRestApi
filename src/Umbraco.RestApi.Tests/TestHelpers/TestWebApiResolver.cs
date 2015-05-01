using System.Collections.Generic;
using System.Reflection;
using System.Web.Http.Dispatcher;
using Umbraco.RestApi.Controllers;

namespace Umbraco.RestApi.Tests.TestHelpers
{
    /// <summary>
    /// Custom resolver to look for controllers in our assembly
    /// </summary>
    public class TestWebApiResolver : IAssembliesResolver
    {     
        public ICollection<Assembly> GetAssemblies()
        {
            return new[] {typeof (ContentController).Assembly};
        }
    }
}