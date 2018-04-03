using YM.Json;

namespace YM.Elasticsearch.Client.Indices
{
    public class CreateIndexRequest : RestRequest
    {
        public string Index { get; private set; }
        public JsonObject Schema { get; private set; }

        public CreateIndexRequest(string index, JsonObject schema = null)
        {
            Index = index;
            Schema = schema ?? DefaultSchema;
        }

        public static JsonObject DefaultSchema
        {
            get
            {
                return new JsonObject()
                    .Add("settings", new JsonObject()
                        .Add("index", new JsonObject()
                            .Add("number_of_shards", 1)
                            .Add("number_of_replicas", 0)))
                    .Add("mappings", new JsonObject()
                        .Add("doc", new JsonObject()
                            .Add("dynamic", true)));
            }
        }

        public override JsonObject ToJson()
        {
            return Schema.Copy().Add("index", Index);
        }

        public override string GetUrl(string clusterUrl)
        {
            return string.Format("{0}/{1}", clusterUrl, Index);
        }

        public override string GetBody()
        {
            return Schema.ToString();
        }
    }
}
