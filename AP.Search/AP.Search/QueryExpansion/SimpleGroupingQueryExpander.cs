using YM.Elasticsearch.Query;
using YM.Elasticsearch.Query.CompoundQueries;
using YM.Elasticsearch.Query.FullTextQueries;

namespace AP.Search.QueryExpansion
{
    public class SimpleGroupingQueryExpander : MultiMatchQueryExpander
    {
        protected override IQuery Expand(MultiMatchQuery query)
        {
            return new BoolQuery()
                .Must(query)
                .Should(new IQuery[] {
                    new MultiMatchQuery(query.Query, query.Fields, MultiMatchType.Phrase),
                    new MultiMatchQuery(query.Query, query.Fields, query.MatchType, true)
                });
        }
    }
}