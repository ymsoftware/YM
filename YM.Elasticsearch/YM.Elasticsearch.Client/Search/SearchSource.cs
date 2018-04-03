using System.Linq;
using YM.Json;

namespace YM.Elasticsearch.Client.Search
{
    public class SearchSource : JsonDocument
    {
        public string[] Includes { get; private set; }
        public string[] Excludes { get; private set; }
        public bool IsHidden { get; private set; }

        public SearchSource(string[] includes, string[] excludes = null)
        {
            Includes = includes;
            Excludes = excludes;
            IsHidden = (includes == null || includes.Length == 0) && (excludes == null || excludes.Length == 0);
        }

        public SearchSource(JsonValue jv)
        {
            if (jv == null)
            {
                return;
            }

            if (jv.Type == JsonType.Boolean && !jv.Get<bool>())
            {
                IsHidden = true;
                return;
            }

            if (jv.Type == JsonType.String)
            {
                Includes = new string[] { jv.Get<string>() };
            }
            else if (jv.Type== JsonType.Array)
            {
                Includes = jv.Get<JsonArray>().Select(e => e.Get<string>()).ToArray();
            }
            else if (jv.Type == JsonType.Object)
            {
                var source = jv.Get<JsonObject>();

                var fields = source.Property<JsonArray>("includes");
                if (fields != null && fields.Length > 0)
                {
                    Includes = fields.Select(e => e.Get<string>()).ToArray();
                }

                fields = source.Property<JsonArray>("excludes");
                if (fields != null && fields.Length > 0)
                {
                    Excludes = fields.Select(e => e.Get<string>()).ToArray();
                }
            }
        }

        public JsonValue ToJsonValue()
        {
            if (IsHidden)
            {
                return false;
            }

            bool includes = Includes != null && Includes.Length > 0;
            if (Excludes != null && Excludes.Length > 0)
            {
                var jo = new JsonObject();

                if (includes)
                {
                    jo.Add("includes", Includes);
                }

                return jo.Add("excludes", Excludes);
            }
            else if (includes)
            {
                if (Includes.Length == 1)
                {
                    return Includes[0];
                }
                else
                {
                    return new JsonValue(Includes);
                }
            }

            return new JsonObject();
        }

        public override JsonObject ToJson()
        {
            return new JsonObject().Add("_source", ToJsonValue());
        }        
    }
}
