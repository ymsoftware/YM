namespace YM.Elasticsearch.Query.TermQueries
{
    public class TermsQuery : FieldQueryBase
    {
        public object[] Values { get; private set; }

        public TermsQuery(string field, object[] values) : base(field, values)
        {
            Values = values;
        }

        public override QueryType Type => QueryType.TermsQuery;
    }
}
