namespace YM.Elasticsearch.Query.FullTextQueries
{
    public class MatchPhrasePrefixQuery : FieldQueryBase
    {
        public MatchPhrasePrefixQuery(string field, string value) : base(field, value) { }

        public override QueryType Type => QueryType.MatchPhrasePrefixQuery;
    }
}
