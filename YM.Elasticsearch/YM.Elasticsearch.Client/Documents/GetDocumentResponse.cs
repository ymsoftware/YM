using YM.Json;

namespace YM.Elasticsearch.Client.Documents
{
    public class GetDocumentResponse : JsonDocument
    {
        public ElasticsearchDocument Document { get; private set; }
        public bool IsSuccess { get; private set; }
        public JsonObject Response { get; private set; }

        public GetDocumentResponse(RestResponse response)
        {
            var jo = JsonObject.Parse(response.Content);

            Document = new ElasticsearchDocument(jo);

            IsSuccess = response.Http.IsSuccessStatusCode
                && (jo.Property<bool>("found"));

            Response = jo;
        }

        public override JsonObject ToJson()
        {
            return Response;
        }
    }
}
