using YM.Elasticsearch.Query;
using YM.Elasticsearch.Query.CompoundQueries;
using YM.Elasticsearch.Query.FullTextQueries;

namespace AP.Search.QueryExpansion
{
    public class ProximityQueryExpander : MultiMatchQueryExpander
    {
        public string Field { get; private set; }
        public int Slop { get; private set; }

        public ProximityQueryExpander(string field, int slop)
        {
            Field = field;
            Slop = slop;
        }
        
        protected override IQuery Expand(MultiMatchQuery query)
        {
            return new BoolQuery()
                .Must(query)
                .Should(new MatchPhraseQuery(Field, query.Query, Slop));
        }
    }
}