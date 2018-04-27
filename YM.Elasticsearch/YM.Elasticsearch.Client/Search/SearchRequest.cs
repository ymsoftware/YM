using System;
using System.Linq;
using YM.Elasticsearch.Query;
using YM.Json;

namespace YM.Elasticsearch.Client.Search
{
    public class SearchRequest : RestRequest
    {
        private const int DEFAULT_SIZE = 10;

        private readonly JsonObject _jo;
        private readonly string _query;

        public string Index { get; private set; }
        public string Type { get; private set; }
        public IQuery Query { get; private set; }
        public int Size { get; private set; }
        public int From { get; private set; }
        public SearchSource Source { get; private set; }
        public SearchSort[] Sort { get; private set; }
        public object[] SearchAfter { get; private set; }

        public SearchRequest()
        {
            Size = DEFAULT_SIZE;
        }

        public SearchRequest(JsonObject jo, bool parseJson = true)
        {
            Size = DEFAULT_SIZE;
            _jo = ParseJson(jo, parseJson);
        }

        public SearchRequest(string searchUri, JsonObject jo, bool parseJson = true)
        {
            Size = DEFAULT_SIZE;
            _jo = ParseJson(jo, parseJson);

            var url = searchUri.Trim('/').Split('?');

            string path = url[0];
            var tokens = path.Split('/');
            int size = tokens.Length;
            if (size > 3 || tokens[size - 1] != "_search")
            {
                throw new Exception(string.Format("Invalid search uri [{0}]", searchUri));
            }

            if (size > 1)
            {
                Index = tokens[0];

                if (size > 2)
                {
                    Type = tokens[1];
                }
            }   

            if (url.Length > 1)
            {
                _query = "?" + url[1];
            }
        }

        public SearchRequest SetIndex(string index)
        {
            Index = index;
            return this;
        }

        public SearchRequest SetType(string type)
        {
            Type = type;
            return this;
        }

        public SearchRequest SetQuery(IQuery query)
        {
            Query = query;
            return this;
        }

        public SearchRequest SetSize(int size)
        {
            Size = size;
            return this;
        }

        public SearchRequest SetFrom(int from)
        {
            From = from;
            return this;
        }

        public SearchRequest SetSource(string[] includes, string[] excludes = null)
        {
            Source = new SearchSource(includes, excludes);
            return this;
        }

        public SearchRequest HideSource()
        {
            Source = new SearchSource(null, null);
            return this;
        }

        public SearchRequest SetSort(string field, bool isDescending = false)
        {
            var sort = new SearchSort(field, isDescending);

            if (Sort == null || Sort.Length == 0)
            {
                Sort = new SearchSort[] { sort };
            }
            else
            {
                int size = Sort.Length;
                var temp = new SearchSort[size + 1];
                Array.Copy(Sort, temp, size);
                temp[size] = sort;
                Sort = temp;
            }

            return this;
        }

        public SearchRequest SetSort(SearchSort[] sort)
        {
            Sort = sort;
            return this;
        }

        public SearchRequest SetSearchAfter(object[] searchAfter)
        {
            SearchAfter = searchAfter;
            return this;
        }

        public override string GetBody()
        {
            return ToJson().ToString();
        }

        public override string GetUrl(string clusterUrl)
        {
            string index = string.IsNullOrWhiteSpace(Index) ? "" : Index;
            string type = string.IsNullOrWhiteSpace(Type) || index.Length == 0 ? "" : "/" + Type;
            return string.Format("{0}/{1}{2}/_search{3}", clusterUrl, Index, type, _query);
        }

        public override JsonObject ToJson()
        {
            if (_jo != null)
            {
                return _jo;
            }

            var jo = new JsonObject();

            if (Query != null)
            {
                jo.Add("query", Query.ToJson());
            }

            if (From > 0)
            {
                jo.Add("from", From);
            }

            if (Size != DEFAULT_SIZE && Size > 0)
            {
                jo.Add("size", Size);
            }

            if (Sort != null && Sort.Length > 0)
            {
                jo.Add("sort", Sort.Select(e => e.ToJson()));
            }

            if (Source != null)
            {
                jo.Add("_source", Source.ToJsonValue());
            }

            if (SearchAfter != null && SearchAfter.Length > 0)
            {
                jo.Add("search_after", SearchAfter);
            }

            return jo;
        }

        private SearchSort[] GetSort(JsonProperty jp)
        {
            if (jp.Value.Type == JsonType.Array)
            {
                var ja = jp.Value.Get<JsonArray>();
                return ja.Length == 0 ? null : ja.Select(e => new SearchSort(e)).ToArray();
            }

            return null;
        }

        private JsonObject ParseJson(JsonObject jo, bool parseJson)
        {
            if (jo != null && !jo.IsEmpty)
            {
                if (parseJson)
                {
                    foreach (var jp in jo.Properties())
                    {
                        switch (jp.Name)
                        {
                            case "query": Query = jp.Value.Get<JsonObject>().ToQuery(); break;
                            case "size": Size = jp.Value.Get<int>(); break;
                            case "from": From = jp.Value.Get<int>(); break;
                            case "sort": Sort = GetSort(jp); break;
                            case "_source": Source = new SearchSource(jp.Value); break;
                            case "search_after": SearchAfter = jp.Value.Get<JsonArray>().Select(e => e.Get()).ToArray(); break;
                        }
                    }
                }
                else
                {
                    return jo;
                }
            }

            return null;
        }
    }
}