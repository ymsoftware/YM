using AP.Search.QueryExpansion;
using System.Collections.Generic;
using YM.Elasticsearch;
using YM.Elasticsearch.Client.Search;
using YM.Json;

namespace AP.Search.SearchTemplates
{
    public class SearchTemplate : JsonDocument
    {
        public const string ID = "id";
        public const string NAME = "name";
        public const string DESCRIPTION = "description";
        public const string INDEX = "index";
        public const string TYPE = "type";
        public const string FIELD_ALIASES = "field_aliases";
        public const string QUERY_EXPAND = "query_expand";

        public const string FIELD_ALIASES_APPL = "appl";

        public readonly string[] ES = new string[] { "es", "request", "search", "search_request" };

        private readonly JsonProperty _aliases;
        private readonly JsonProperty _qe;

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Index { get; private set; }
        public string Type { get; private set; }
        public IDictionary<string, string[]> FieldAliases { get; private set; }
        public IQueryExpander QueryExpander { get; private set; }
        public JsonObject RequestBody { get; private set; }

        public SearchTemplate(JsonObject jo)
        {
            Id = jo.Property<string>(ID);
            Name = jo.Property<string>(NAME);
            Description = jo.Property<string>(DESCRIPTION);
            Index = jo.Property<string>(INDEX);
            Type = jo.Property<string>(TYPE);

            var es = jo.Property(ES).Value;
            if (es != null)
            {
                if (es.Type == JsonType.String)
                {
                    RequestBody = JsonObject.Parse(es.Get<string>());
                }
                else
                {
                    RequestBody = es.Get<JsonObject>();
                }
            }

            var jp = jo.Property(FIELD_ALIASES);
            if (jp != null)
            {
                IDictionary<string, string[]> aliases = null;
                JsonObject values = null;

                if (jp.Value.Type == JsonType.String)
                {
                    string value = jp.Value.Get<string>();
                    if (value.StartsWith("{"))
                    {
                        values = JsonObject.Parse(value);                        
                    }
                    else
                    {
                        aliases = FieldAliasesRepository.GetAliases(value);
                    }
                }
                else if (jp.Value.Type == JsonType.Object)
                {
                    values = jp.Value.Get<JsonObject>();
                }

                if (values != null)
                {
                    aliases = FieldAliasesRepository.GetAliases(values);
                    jp = new JsonProperty(FIELD_ALIASES, values);
                }

                if (aliases != null)
                {
                    FieldAliases = aliases;
                    _aliases = jp;
                }
            }

            jp = jo.Property(QUERY_EXPAND);
            if (jp != null)
            {
                IQueryExpander qe = null;
                JsonObject values = null;

                if (jp.Value.Type == JsonType.String)
                {
                    string value = jp.Value.Get<string>();
                    if (value.StartsWith("{"))
                    {
                        values = JsonObject.Parse(value);
                    }
                    else
                    {
                        qe = QueryExpanderRepository.GetQueryExpander(value);
                    }
                }
                else if (jp.Value.Type == JsonType.Object)
                {
                    values = jp.Value.Get<JsonObject>();
                }

                if (values != null)
                {
                    qe = QueryExpanderRepository.GetQueryExpander(values);
                    jp = new JsonProperty(QUERY_EXPAND, values);
                }
                
                if (qe != null)
                {
                    QueryExpander = qe;
                    _qe = jp;
                }
            }
        }

        public SearchRequest GetSearchRequest(IDictionary<string, object> parameters)
        {
            var jo = ToJson().SetParameters(parameters);

            var template = new SearchTemplate(jo);

            var request = new SearchRequest(template.RequestBody, true)
                .SetIndex(template.Index)
                .SetType(template.Type);
            
            if (request.Query != null)
            {
                request.SetQuery(request.Query.Transform(template.FieldAliases, template.QueryExpander));
            }

            return request;
        }

        public override JsonObject ToJson()
        {
            return ToJson(false);
        }

        public JsonObject ToJson(bool sourceAsString)
        {
            var jo = new JsonObject()
                .Add(ID, Id)
                .Add(NAME, Name);

            if (!string.IsNullOrWhiteSpace(Description))
            {
                jo.Add(DESCRIPTION, Description);
            }

            jo.Add(INDEX, Index);

            if (!string.IsNullOrWhiteSpace(Type))
            {
                jo.Add(TYPE, Type);
            }

            if (sourceAsString)
            {
                jo.Add(ES[0], RequestBody.ToString(false));
            }
            else
            {
                jo.Add(ES[0], RequestBody);
            }

            if (_aliases != null)
            {
                if (sourceAsString && _aliases.Value.Type == JsonType.Object)
                {
                    jo.Add(FIELD_ALIASES, _aliases.Value.Get<JsonObject>().ToString(false));
                }
                else
                {
                    jo.Add(_aliases);
                }                
            }

            if (_qe != null)
            {
                if (sourceAsString && _qe.Value.Type == JsonType.Object)
                {
                    jo.Add(QUERY_EXPAND, _qe.Value.Get<JsonObject>().ToString(false));
                }
                else
                {
                    jo.Add(_qe);
                }                
            }

            return jo;
        }
    }
}
