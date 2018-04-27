using Nancy;
using YM.Elasticsearch;
using YM.Json;

namespace AP.Search.Web.Nancy.Modules.Api
{
    public class ApiResponse : JsonDocument
    {
        public HttpStatusCode StatusCode { get; private set; }
        public string Content { get; private set; }
        public string Log { get; private set; }

        public ApiResponse(string content, HttpStatusCode statusCode = HttpStatusCode.OK, string log = null)
        {
            Content = content;
            StatusCode = statusCode;
            Log = log == null ? content : log;
        }

        public Response ToResponse()
        {
            var response = (Response)Content;
            response.ContentType = "application/json";
            response.StatusCode = StatusCode;
            return response;
        }

        public override JsonObject ToJson()
        {
            return new JsonObject()
                .Add("status", StatusCode)
                .Add("content", Content)
                .Add("log", Log);
        }
    }
}