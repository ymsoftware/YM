using YM.Json;

namespace YM.Elasticsearch.Query.CompoundQueries
{
    public class ConstantScoreQuery : QueryBase
    {
        public IQuery Query { get; private set; }

        public ConstantScoreQuery(IQuery query)
        {
            Query = query;
        }

        public override QueryType Type => QueryType.ConstantScoreQuery;

        public override JsonObject ToJson()
        {
            return new JsonObject().Add("constant_score", new JsonObject().Add("filter", Query.ToJson()));
        }
    }
}