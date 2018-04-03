using YM.Json;

namespace YM.Elasticsearch.Client.Documents
{
    public class DeleteDocumentResponse : JsonDocument
    {
        public string Id { get; private set; }
        public string Index { get; private set; }
        public string Type { get; private set; }
        public int Version { get; private set; }
        public bool IsSuccess { get; private set; }
        public string Result { get; private set; }
        public JsonObject Response { get; private set; }

        public DeleteDocumentResponse(RestResponse response)
        {
            var jo = JsonObject.Parse(response.Content);

            Id = jo.Property<string>("_id");
            Index = jo.Property<string>("_index");
            Type = jo.Property<string>("_type");
            Version = jo.Property<int>("_version");
            Result = jo.Property<string>("result");

            IsSuccess = response.Http.IsSuccessStatusCode
                && Result == "deleted";

            Response = jo;
        }

        public override JsonObject ToJson()
        {
            return Response;
        }
    }
}
