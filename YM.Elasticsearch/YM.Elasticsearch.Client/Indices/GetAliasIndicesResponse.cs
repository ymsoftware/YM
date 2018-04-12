using System.Linq;
using YM.Json;

namespace YM.Elasticsearch.Client.Indices
{
    public class GetAliasIndicesResponse : JsonDocument
    {
        public bool IsSuccess { get; private set; }
        public string[] Indices { get; private set; }
        public JsonObject Response { get; private set; }

        public GetAliasIndicesResponse(RestResponse response)
        {            
            bool success = response.Http.IsSuccessStatusCode;

            var jo = JsonObject.Parse(response.Content);

            if (success)
            {
                Indices = jo.Properties()
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
