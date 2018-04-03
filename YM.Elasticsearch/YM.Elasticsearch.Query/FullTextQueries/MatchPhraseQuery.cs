namespace YM.Elasticsearch.Query.FullTextQueries
{
    public class MatchPhraseQuery : FieldQueryBase
    {
        public MatchPhraseQuery(string field, string value) : base(field, value) { }

        public override QueryType Type => QueryType.MatchPhraseQuery;
    }
}
