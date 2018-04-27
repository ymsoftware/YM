using System.Linq;
using YM.Elasticsearch.Query;
using YM.Elasticsearch.Query.FullTextQueries;
using YM.Elasticsearch.Query.FullTextQueries.QueryString;

namespace AP.Search.QueryExpansion
{
    public abstract class MultiMatchQueryExpander : IQueryExpander
    {
        public IQuery Expand(IQuery query)
        {
            if (query is MultiMatchQuery)
            {
                var match = query as MultiMatchQuery;

                var qs = match.Query.ToQueryString();
                if (qs.Tokens.Count(e => e.Type == QueryStringTokenType.Term) < 2)
                {
                    return query;
                }

                return Expand(match);
            }

            return query;
        }

        protected abstract IQuery Expand(MultiMatchQuery query);
    }
}