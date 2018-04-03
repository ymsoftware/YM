using YM.Json;

namespace YM.Elasticsearch.Query.FullTextQueries
{
    public class SimpleQueryStringQuery : QueryBase
    {
        public string Query { get; private set; }
        public bool IsAnd { get; private set; }
        public string[] Fields { get; private set; }

        public SimpleQueryStringQuery(string query, string[] fields = null, bool isAnd = false)
        {
            Query = query.Trim();
            Fields = fields;
            IsAnd = isAnd;
        }

        public override QueryType Type => QueryType.SimpleQueryStringQuery;

        public override JsonObject ToJson()
        {
            var query = new JsonObject().Add("query", Query);

            if (Fields != null && Fields.Length > 0)
            {
                query.Add("fields", Fields);
            }

            if (Query.Has(' ') && !Query.IsPhrase() && IsAnd)
            {
                query.Add("default_operator", "and");
            }

            return new JsonObject().Add("simple_query_string", query);
        }
    }
}
