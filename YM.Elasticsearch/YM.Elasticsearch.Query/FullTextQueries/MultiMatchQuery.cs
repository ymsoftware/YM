using YM.Json;

namespace YM.Elasticsearch.Query.FullTextQueries
{
    public class MultiMatchQuery : QueryBase
    {
        public string[] Fields { get; private set; }
        public string Query { get; private set; }
        public MultiMatchType MatchType { get; private set; }
        public bool IsAnd { get; private set; }
        public double TieBreaker { get; private set; }

        public MultiMatchQuery(string query, string[] fields, MultiMatchType type = MultiMatchType.BestFields, bool isAnd = false, double tieBreaker = 0.0)
        {
            Fields = fields;
            Query = query;
            MatchType = type;
            IsAnd = isAnd;
            TieBreaker = tieBreaker;
        }

        public override QueryType Type => QueryType.MultiMatchQuery;

        public override JsonObject ToJson()
        {
            var query = new JsonObject()
                .Add("query", Query)
                .Add("fields", Fields);

            switch (MatchType)
            {
                case MultiMatchType.CrossFields: query.Add("type", "cross_fields"); break;
                case MultiMatchType.MostFields: query.Add("type", "most_fields"); break;
                case MultiMatchType.Phrase: query.Add("type", "phrase"); break;
                case MultiMatchType.PhrasePrefix: query.Add("type", "phrase_prefix"); break;
            }

            if (IsAnd)
            {
                query.Add("operator", "and");
            }

            if (TieBreaker > 0.0)
            {
                query.Add("tie_breaker", TieBreaker);
            }

            return new JsonObject().Add("multi_match", query);
        }
    }
}
