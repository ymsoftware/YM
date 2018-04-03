namespace YM.Elasticsearch.Query
{
    static class QueryTypeExtensions
    {
        public static string ToQueryName(this QueryType type)
        {
            switch (type)
            {
                case QueryType.MatchAllQuery: return "match_all";
                case QueryType.TermQuery: return "term";
                case QueryType.TermsQuery: return "terms";
                case QueryType.PrefixQuery: return "prefix";
                case QueryType.WildcardQuery: return "wildcard";
                case QueryType.RangeQuery: return "range";
                case QueryType.ExistsQuery: return "exists";
                case QueryType.MatchQuery: return "match";
                case QueryType.MatchPhraseQuery: return "match_phrase";
                case QueryType.MatchPhrasePrefixQuery: return "match_phrase_prefix";
                case QueryType.QueryStringQuery: return "query_string";
                case QueryType.SimpleQueryStringQuery: return "simple_query_string";
                case QueryType.MultiMatchQuery: return "multi_match";
                case QueryType.BoolQuery: return "bool";
                case QueryType.ConstantScoreQuery: return "constant_score";
                case QueryType.FunctionScoreQuery: return "function_score";
                case QueryType.PercolateQuery: return "percolate";                
            }

            return null;
        }
    }
}