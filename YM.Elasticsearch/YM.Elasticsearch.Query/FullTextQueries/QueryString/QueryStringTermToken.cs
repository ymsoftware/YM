namespace YM.Elasticsearch.Query.FullTextQueries.QueryString
{
    public class QueryStringTermToken : QueryStringToken
    {
        public string Term { get; private set; }

        public QueryStringTermToken(string term) : 
            base(QueryStringTokenType.Term)
        {
            Term = term;
        }

        public override string ToString()
        {
            return Term;
        }
    }
}
