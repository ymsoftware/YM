using YM.Json;

namespace YM.Elasticsearch.Client.Documents
{
    public class BulkResponse : JsonDocument
    {
        public bool IsSuccess { get; private set; }
        public bool HasErrors { get; private set; }
        public JsonObject Response { get; private set; }

        public BulkResponse(RestResponse response)
        {            
            IsSuccess = response.Http.IsSuccessStatusCode;

            var jo = JsonObject.Parse(response.Content);
            Response = jo;
            HasErrors = jo.Property<bool>("errors");
        }

        public override JsonObject ToJson()
        {
            return Response;
        }
    }
}
