using YM.Json;

namespace YM.Elasticsearch
{
    public class ElasticsearchDocument : JsonDocument
    {
        public string Id { get; private set; }
        public string Index { get; private set; }
        public JsonObject Source { get; private set; }
        public string Type { get; private set; }
        public int Version { get; private set; }

        public ElasticsearchDocument(string id, JsonObject source, string index, string type = "doc", int version = 0)
        {
            Id = id;
            Index = index;
            Source = source;
            Type = type;
            Version = version;
        }

        public ElasticsearchDocument(JsonObject document)
        {
            Id = document.Property<string>("_id");
            Index = document.Property<string>("_index");
            Source = document.Property<JsonObject>("_source");
            Type = document.Property<string>("_type");
            Version = document.Property<int>("_version");
        }

        public override JsonObject ToJson()
        {
            var jo = new JsonObject()
                .Add("_id", Id)
                .Add("_index", Index)
                .Add("_type", Type);

            if (Version > 0)
            {
                jo.Add("_version", Version);
            }

            jo.Add("_source", Source);

            return jo;
        }
    }
}