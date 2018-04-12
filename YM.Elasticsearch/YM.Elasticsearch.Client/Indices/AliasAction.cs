using YM.Json;

namespace YM.Elasticsearch.Client.Indices
{
    public class AliasAction : JsonDocument
    {
        public string Action { get; private set; }
        public string Index { get; private set; }
        public string Alias { get; private set; }

        public AliasAction(string action, string index, string alias)
        {
            Action = action;
            Index = index;
            Alias = alias;
        }

        public override JsonObject ToJson()
        {
            return new JsonObject()
                .Add(Action, new JsonObject()
                    .Add("index", Index)
                    .Add("alias", Alias));
        }
    }
}
