namespace YM.Elasticsearch.Query.FullTextQueries.QueryString
{
    public class QueryStringToken
    {
        public QueryStringTokenType Type { get; private set; }

        public QueryStringToken(QueryStringTokenType type)
        {
            Type = type;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case QueryStringTokenType.And: return "AND";
                case QueryStringTokenType.Or: return "OR";
                case QueryStringTokenType.Not: return "NOT";
                case QueryStringTokenType.Must: return "+";
                case QueryStringTokenType.MustNot: return "-";
                case QueryStringTokenType.Fuzzy: return ((QueryStringFuzzyToken)this).ToString();
                case QueryStringTokenType.Boost: return ((QueryStringBoostToken)this).ToString();
                case QueryStringTokenType.Term: return ((QueryStringTermToken)this).ToString();
                case QueryStringTokenType.Query: return ((QueryStringQueryToken)this).ToString();
                case QueryStringTokenType.Group: return string.Format("({0})", (QueryStringGroupToken)this);
            }

            return "";
        }
    }
}
