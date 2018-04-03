namespace YM.Elasticsearch.Query.TermQueries
{
    public class ExistsQuery : FieldQueryBase
    {
        public ExistsQuery(string field) : base("field", field) { }

        public override QueryType Type => QueryType.ExistsQuery;
    }
}
