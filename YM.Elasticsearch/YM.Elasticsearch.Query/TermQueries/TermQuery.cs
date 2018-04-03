namespace YM.Elasticsearch.Query.TermQueries
{
    public class TermQuery : FieldQueryBase
    {
        public TermQuery(string field, object value) : base(field, value) { }

        public override QueryType Type => QueryType.TermQuery;
    }
}
