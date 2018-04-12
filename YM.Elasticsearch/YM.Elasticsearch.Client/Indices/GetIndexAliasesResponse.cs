using System.Linq;
using YM.Json;

namespace YM.Elasticsearch.Client.Indices
{
    public class GetIndexAliasesResponse : JsonDocument
    {
        public bool IsSuccess { get; private set; }
        public string[] Aliases { get; private set; }
        public JsonObject Response { get; private set; }

        public GetIndexAliasesResponse(RestResponse response)
        {            
            bool success = response.Http.IsSuccessStatusCode;

            var jo = JsonObject.Parse(response.Content);

            if (success)
            {
                Aliases = jo
                    .Properties()[0].Value.Get<JsonObject>()
                    .Property<JsonObject>("aliases")
                    .Properties()
                    .Select(e => e.Name)
                    .ToArray();
            }

            IsSuccess = success;
            Response = jo;            
        }

        public override JsonObject ToJson()
        {
            return Response;
        }
    }
}
