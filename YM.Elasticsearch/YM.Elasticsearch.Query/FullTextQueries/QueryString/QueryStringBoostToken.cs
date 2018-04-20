namespace YM.Elasticsearch.Query.FullTextQueries.QueryString
{
    public class QueryStringBoostToken : QueryStringToken
    {
        public int Boost { get; private set; }

        public QueryStringBoostToken(int boost) : 
            base(QueryStringTokenType.Boost)
        {
            Boost = boost;
        }

        public override string ToString()
        {
            return string.Format("^{0}", Boost);
        }
    }
}
