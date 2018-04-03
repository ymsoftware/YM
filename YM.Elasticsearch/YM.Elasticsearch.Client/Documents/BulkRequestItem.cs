using YM.Json;

namespace YM.Elasticsearch.Client.Documents
{
    public class BulkRequestItem: JsonDocument
    {
        public string Action { get; private set; }
        public string Id { get; private set; }
        public string Index { get; private set; }
        public string Type { get; private set; }
        public JsonObject Source { get; private set; }
        public bool IsRefresh { get; private set; }
        public int Version { get; private set; }

        public BulkRequestItem(string action, string id, string index, string type, JsonObject source, int version = 0)
        {
            Action = action;
            Id = id;
            Index = index;
            Type = type;
            Source = source;
            Version = version;
        }

        public override JsonObject ToJson()
        {
            var jo = ActionToJson();

            if (Source != null && !Source.IsEmpty)
            {
                jo.Add("_source", Source);
            }

            return jo;
        }

        public string ToBulkString()
        {
            string action = ActionToJson().ToString(false);

            if (Source == null || Source.IsEmpty)
            {
                return action;
            }

            return string.Format("{0}\n{1}", action, Source.ToString(false));
        }

        private JsonObject ActionToJson()
        {
            var action = new JsonObject()
                .Add("_id", Id)
                .Add("_index", Index)
                .Add("_type", Type);

            if (Version > 0)
            {
                action.Add("_version", Version);
            };

            return new JsonObject()
                .Add(Action, action);
        }
    }
}
