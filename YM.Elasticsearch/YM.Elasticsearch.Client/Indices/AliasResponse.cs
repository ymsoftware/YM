using YM.Json;

namespace YM.Elasticsearch.Client.Indices
{
    public class AliasResponse : JsonDocument
    {
        public bool IsSuccess { get; private set; }
        public JsonObject Response { get; private set; }

        public AliasResponse(RestResponse response)
        {
            var jo = JsonObject.Parse(response.Content);

            IsSuccess = response.Http.IsSuccessStatusCode
                && jo.Property<bool>("acknowledged");

            Response = jo;            
        }

        public override JsonObject ToJson()
        {
            return Response;
        }
    }
}
