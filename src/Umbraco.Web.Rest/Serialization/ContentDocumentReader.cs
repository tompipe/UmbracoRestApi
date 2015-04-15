using System;
using System.Collections.Generic;
using System.Linq;
using CollectionJson;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;


namespace Umbraco.Web.Rest.Serialization
{
    public class ContentDocumentReader : ICollectionJsonDocumentReader<IContent>
    {
        private readonly IContentService _contentService;
        private readonly ILogger _logger;

        public ContentDocumentReader(IContentService contentService, ILogger logger)
        {
            _contentService = contentService;
            _logger = logger;
        }

        public IContent Read(IWriteDocument document)
        {
            if (document == null) throw new ArgumentNullException("document");
            if (document.Template == null) throw new ArgumentNullException("document.Template", "document.Template cannot be null");
            
            var template = document.Template;

            var contentType = template.Data.GetRequiredDataValueByName<string>(FieldNames.ContentTypeAlias);
            var name = template.Data.GetRequiredDataValueByName<string>(FieldNames.Name);

            //get a temp content item based on the content type alias
            var content = _contentService.CreateContent(name, -1, contentType);
        
            var properties = template.Data.GetDataByName(FieldNames.Properties);
            //no properties to save, just return
            if (properties == null) return content;

            var propertyList = FromCustomArray(properties, "array");

            //no properties to save, just return
            if (propertyList == null) return content;
            
            //iterate through the posted properties and update the value of the real property if it exists 
            // in the content item
            foreach (var propertyItem in propertyList)
            {
                var propertyData = FromCustomArray(propertyItem, "data");
                if (propertyData != null)
                {
                    //get the alias/value
                    var alias = propertyData.GetRequiredDataValueByName<string>(FieldNames.Alias);
                    var value = propertyData.GetRequiredDataValueByName<string>(FieldNames.Value);
                            
                    //find the real property
                    var property = content.Properties[alias];
                    if (property != null)
                    {
                        property.Value = value;
                    }
                }
            }

            return content;
        }


        private List<Data> FromCustomArray(Data data, string propertyName)
        {
            List<Data> propertyList = null;
            //the incoming custom 'array' will be JArray in most cases, we'll do a check
            var propertyListArray = data.GetValue<object>(propertyName);
            var jArray = propertyListArray as JArray;
            if (jArray != null)
            {
                propertyList = jArray.ToObject<List<Data>>();
            }
            var list = propertyListArray as List<Data>;
            if (list != null)
            {
                propertyList = list;
            }
            if (propertyList == null)
            {
                _logger.Warn<ContentDocumentReader>("The format of the array is not supported: " + propertyListArray.GetType());
            }
            return propertyList;
        }
    }

    
}