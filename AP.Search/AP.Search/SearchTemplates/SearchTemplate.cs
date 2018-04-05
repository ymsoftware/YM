using System.Collections.Generic;
using YM.Elasticsearch;
using YM.Elasticsearch.Client.Search;
using YM.Json;

namespace AP.Search.SearchTemplates
{
    public class SearchTemplate : JsonDocument
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Index { get; private set; }
        public string Type { get; private set; }
        public JsonObject Source { get; private set; }

        public SearchTemplate(JsonObject jo)
        {
            Id = jo.Property<string>("id");
            Name = jo.Property<string>("name");
            Description = jo.Property<string>("description");
            Index = jo.Property<string>("index");
            Type = jo.Property<string>("type");

            var source = jo.Property("source").Value;
            if (source.Type == JsonType.String)
            {
                Source = JsonObject.Parse(source.Get<string>());
            }
            else
            {
                Source = source.Get<JsonObject>();
            }
        }

        public SearchRequest GetSearchRequest(IDictionary<string, object> parameters)
        {
            var jo = Source.SetParameters(parameters);
            return new SearchRequest(Index, Type, jo, true);
        }

        public override JsonObject ToJson()
        {
            return ToJson(false);
        }

        public JsonObject ToJson(bool sourceAsString)
        {
            var jo = new JsonObject()
                .Add("id", Id)
                .Add("name", Name);

            if (!string.IsNullOrWhiteSpace(Description))
            {
                jo.Add("description", Description);
            }

            jo.Add("index", Index);

            if (!string.IsNullOrWhiteSpace(Type))
            {
                jo.Add("type", Type);
            }

            if (sourceAsString)
            {
                jo.Add("source", Source.ToString());
            }
            else
            {
                jo.Add("source", Source);
            }            

            return jo;
        }
    }
}
