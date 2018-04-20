namespace YM.Elasticsearch.Query.FullTextQueries.QueryString
{
    public enum QueryStringTokenType
    {
        Term,
        Query,
        Group,
        And,
        Or,
        Not,
        Must,
        MustNot,
        Boost,
        Fuzzy
    }
}
