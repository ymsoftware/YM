namespace YM.Elasticsearch.Query.TermQueries
{
    public class PrefixQuery : FieldQueryBase
    {
        public PrefixQuery(string field, string value) : base(field, value) { }

        public override QueryType Type => QueryType.PrefixQuery;
    }
}
