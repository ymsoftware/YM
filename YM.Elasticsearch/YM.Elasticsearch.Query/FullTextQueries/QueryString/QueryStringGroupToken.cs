namespace YM.Elasticsearch.Query.FullTextQueries.QueryString
{
    public class QueryStringGroupToken : QueryStringToken
    {
        public QueryString Group { get; private set; }

        public QueryStringGroupToken(QueryString group) : 
            base(QueryStringTokenType.Group)
        {
            Group = group;
        }

        public override string ToString()
        {
            return Group.ToString();
        }
    }
}