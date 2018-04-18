using YM.Json;

namespace YM.Elasticsearch
{
    public class ElasticsearchDocument : JsonDocument
    {
        public const string DEFAULT_TYPE = "doc";
        public const string ID = "_id";
        public const string INDEX = "_index";
        public const string SOURCE = "_source";
        public const string TYPE = "_type";
        public const string VERSION = "_version";

        public string Id { get; private set; }
        public string Index { get; private set; }
        public JsonObject Source { get; private set; }
        public string Type { get; private set; }
        public int Version { get; private set; }

        public ElasticsearchDocument(string id, JsonObject source, string index, string type = null, int version = 0)
        {
            Id = id;
            Index = index;
            Source = source;
            Type = string.IsNullOrWhiteSpace(type) ? DEFAULT_TYPE : type;
            Version = version;
        }

        public ElasticsearchDocument(JsonObject document)
        {
            Id = document.Property<string>(ID);
            Index = document.Property<string>(INDEX);
            Source = document.Property<JsonObject>(SOURCE);
            Type = document.Property<string>(TYPE);
            Version = document.Property<int>(VERSION);
        }

        public override JsonObject ToJson()
        {
            var jo = new JsonObject()
                .Add(ID, Id)
                .Add(INDEX, Index)
                .Add(TYPE, Type);

            if (Version > 0)
            {
                jo.Add(VERSION, Version);
            }

            jo.Add(SOURCE, Source);

            return jo;
        }
    }
}