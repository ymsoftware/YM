using System.Text;

namespace YM.Elasticsearch.Query.FullTextQueries.QueryString
{
    public class QueryString
    {
        public QueryStringToken[] Tokens { get; private set; }

        public QueryString(QueryStringToken[] tokens)
        {
            Tokens = tokens;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            QueryStringToken prev = null;

            foreach (var token in Tokens)
            {
                bool nospace = prev == null 
                    || prev.Type == QueryStringTokenType.Must 
                    || prev.Type == QueryStringTokenType.MustNot
                    || token.Type == QueryStringTokenType.Fuzzy
                    || token.Type == QueryStringTokenType.Boost;

                if (!nospace)
                {
                    sb.Append(" ");
                }

                sb.Append(token.ToString());

                prev = token;
            }

            return sb.ToString();
        }
    }
}