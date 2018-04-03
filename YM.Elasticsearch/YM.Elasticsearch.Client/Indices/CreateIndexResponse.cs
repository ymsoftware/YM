using YM.Json;

namespace YM.Elasticsearch.Client.Indices
{
    public class CreateIndexResponse : JsonDocument
    {
        public string Index { get; private set; }
        public bool IsSuccess { get; private set; }
        public JsonObject Response { get; private set; }

        public CreateIndexResponse(RestResponse response)
        {
            var jo = JsonObject.Parse(response.Content);

            IsSuccess = response.Http.IsSuccessStatusCode
                && jo.Property<bool>("acknowledged") 
                && jo.Property<bool>("shards_acknowledged");

            Index = jo.Property<string>("index");

            Response = jo;            
        }

        public override JsonObject ToJson()
        {
            return Response;
        }
    }
}
