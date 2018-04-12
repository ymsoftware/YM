using YM.Json;

namespace YM.Elasticsearch.Query.SpanQueries
{
    public class SpanMultiTermQuery : QueryBase
    {
        public IQuery Query { get; private set; }

        public override QueryType Type => QueryType.SpanMultiTermQuery;

        public SpanMultiTermQuery(IQuery query)
        {
            Query = query;
        }

        public override JsonObject ToJson()
        {
            return new JsonObject()
                .Add("span_multi", new JsonObject()
                    .Add("match", Query.ToJson()));
        }
    }
}
