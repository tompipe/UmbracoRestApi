using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Umbraco.Core;

namespace Umbraco.RestApi.Controllers
{
    public class CustomActionSelector : ApiControllerActionSelector
    {
        public override HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            object action,allvalues;
            var hasAction = controllerContext.RouteData.Values.TryGetValue("action", out allvalues);
            controllerContext.RouteData.Values.TryGetValue("allvalues", out allvalues);
            var method = controllerContext.Request.Method;

            //nope, wont' work
            if (!hasAction) return base.SelectAction(controllerContext);

            var mappings = GetActionMapping(controllerContext.ControllerDescriptor);

            if (allvalues == null)
            {
                //Action with no id
                return FindAction(method.Method, mappings, controllerContext, 0);
            }

            //not supported, only Get methods have parameters (currently)
            if (method != HttpMethod.Get) return base.SelectAction(controllerContext);

            var valueParts = allvalues.ToString().Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if (valueParts.Length == 0) return base.SelectAction(controllerContext);

            //TODO: Will need to support GUID too
            int id;
            if (!int.TryParse(valueParts[0], out id)) return base.SelectAction(controllerContext);

            //populate Id
            controllerContext.RouteData.Values["id"] = id;

            if (valueParts.Length == 1)
            {
                //Get with Id
                return FindAction(method.Method, mappings, controllerContext, 1);
            }
            if (valueParts.Length == 2)
            {
                //Get with Id + traversal (i.e. /123/children )
                return FindAction(valueParts[1], mappings, controllerContext);
            }

            
            return base.SelectAction(controllerContext);
        }

        private HttpActionDescriptor FindAction(
            string actionName, 
            ILookup<string, HttpActionDescriptor> mappings, 
            HttpControllerContext controllerContext,
            int? parameterCount = null)
        {
            var map = mappings.FirstOrDefault(x => x.Key.InvariantEquals(actionName));
            if (map == null) return base.SelectAction(controllerContext);
            if (parameterCount.HasValue)
            {
                var found = map.FirstOrDefault(a => a.ActionBinding.ParameterBindings.Count() == parameterCount);
                if (found == null) return base.SelectAction(controllerContext);
                return found;
            }
            else
            {
                var found = map.FirstOrDefault();
                if (found == null) return base.SelectAction(controllerContext);
                return found;
            }
            
        }
    }
}