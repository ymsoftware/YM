namespace YM.Elasticsearch.Query.FullTextQueries.QueryString
{
    public class QueryStringFuzzyToken : QueryStringToken
    {
        public int Proximity { get; private set; }

        public QueryStringFuzzyToken(int proximity) : 
            base(QueryStringTokenType.Fuzzy)
        {
            Proximity = proximity;
        }

        public override string ToString()
        {
            return Proximity > 0 ? string.Format("~{0}", Proximity) : "~";
        }
    }
}
