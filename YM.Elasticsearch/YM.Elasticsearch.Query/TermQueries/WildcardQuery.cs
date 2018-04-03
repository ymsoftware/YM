namespace YM.Elasticsearch.Query.TermQueries
{
    public class WildcardQuery : FieldQueryBase
    {
        public WildcardQuery(string field, string value) : base(field, value) { }

        public override QueryType Type => QueryType.WildcardQuery;
    }
}
