using System.Text;
using YM.Elasticsearch.Query.TermQueries;

namespace YM.Elasticsearch.Query.FullTextQueries.QueryString
{
    public class QueryStringQueryToken : QueryStringToken
    {
        public IQuery Query { get; private set; }

        public QueryStringQueryToken(MatchQuery query) :
            base(QueryStringTokenType.Query)
        {
            Query = query;
        }

        public QueryStringQueryToken(RangeQuery query) :
            base(QueryStringTokenType.Query)
        {
            Query = query;
        }

        public override string ToString()
        {
            if (Query is RangeQuery)
            {
                var range = Query as RangeQuery;

                var sb = new StringBuilder()
                    .Append(range.Field)
                    .Append(":");

                if (range.Values.From == null || range.Values.From.Value == null)
                {
                    sb.Append("<");
                    if (range.Values.To.Include)
                    {
                        sb.Append("=");
                    }
                    sb.Append(range.Values.To.Value);
                }
                else if (range.Values.To == null || range.Values.To.Value == null)
                {
                    sb.Append(">");
                    if (range.Values.From.Include)
                    {
                        sb.Append("=");
                    }
                    sb.Append(range.Values.From.Value);
                }
                else
                {
                    sb
                        .Append(range.Values.From.Include ? "[" : "{")    
                        .Append(range.Values.From.Value)
                        .Append(" TO ")
                        .Append(range.Values.To.Value)
                        .Append(range.Values.To.Include ? "]" : "}");
                }

                return sb.ToString();
            }
            else
            {
                var match = Query as MatchQuery;
                return string.Format("{0}:{1}", match.Field, match.Value);
            }
        }
    }
}
