using System.Net.Http;
using CollectionJson;
using CollectionJson.Client;

namespace Umbraco.Web.Rest
{
    public static class ReadDocumentExtensions
    {
        private static readonly CollectionJsonFormatter _formatter;

        static ReadDocumentExtensions()
        {
            _formatter = new CollectionJsonFormatter();
        }

        public static ObjectContent ToObjectContent(this IReadDocument document)
        {
            return new ObjectContent<IReadDocument>(document, _formatter, "application/vnd.collection+json");
        }

        public static HttpResponseMessage ToHttpResponseMessage(this IReadDocument document)
        {
            var response = new HttpResponseMessage();
            response.Content = document.ToObjectContent();
            return response;
        }
    }
}
