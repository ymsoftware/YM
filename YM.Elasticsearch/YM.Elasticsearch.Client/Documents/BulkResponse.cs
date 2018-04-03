using YM.Json;

namespace YM.Elasticsearch.Client.Documents
{
    public class BulkResponse : JsonDocument
    {
        public bool IsSuccess { get; private set; }
        public JsonObject Response { get; private set; }

        public BulkResponse(RestResponse response)
        {
            var jo = JsonObject.Parse(response.Content);

            IsSuccess = response.Http.IsSuccessStatusCode
                && (!jo.Property<bool>("errors"));

            Response = jo;
        }

        public override JsonObject ToJson()
        {
            return Response;
        }
    }
}
