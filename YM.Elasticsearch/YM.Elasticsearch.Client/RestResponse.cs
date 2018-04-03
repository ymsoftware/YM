using System.Net.Http;

namespace YM.Elasticsearch.Client
{
    public class RestResponse
    {
        public HttpResponseMessage Http { get; private set; }
        public string Content { get; private set; }

        public RestResponse(HttpResponseMessage http, string content)
        {
            Http = http;
            Content = content;
        }

        public override string ToString()
        {
            return Content;
        }
    }
}
