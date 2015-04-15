using System;
using System.Collections.Generic;
using System.Linq;
using CollectionJson;
using Umbraco.Core;

namespace Umbraco.Web.Rest
{
    public static class CollectionJsonExtensions
    {
        public static T GetRequiredDataValueByName<T>(this IEnumerable<Data> data, string name)
        {
            var foundData = data.GetDataByName(name);
            if (foundData == null) throw new ArgumentException("Required Data not found with name " + name);
            var converted = foundData.Value.TryConvertTo<T>();
            if (!converted.Success) throw new ArgumentException("Data with name " + name + " could not be parsed into type " + typeof(T));
            return converted.Result;
        }

        public static Attempt<T> GetDataValueByName<T>(this IEnumerable<Data> data, string name)
        {
            var foundData = data.GetDataByName(name);
            if (foundData == null) return Attempt<T>.Fail();

            var converted = foundData.Value.TryConvertTo<T>();
            return converted;
        }
    }
}