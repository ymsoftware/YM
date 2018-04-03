using YM.Json;

namespace YM.Elasticsearch.Query.CompoundQueries
{
    public class FilterScoreFunction : QueryBase, IScoreFunction
    {
        public IQuery Query { get; private set; }
        public double Weight { get; private set; }

        public FilterScoreFunction(IQuery query, double weigth)
        {
            Query = query;
            Weight = weigth;
        }

        public override QueryType Type => QueryType.FilterScoreFunction;

        public override JsonObject ToJson()
        {
            return new JsonObject()
                .Add("filter", Query.ToJson())
                .Add("weight", Weight);
        }
    }
}
