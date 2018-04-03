using System.Linq;
using YM.Json;

namespace YM.Elasticsearch.Query.CompoundQueries
{
    public class FunctionScoreQuery : QueryBase
    {
        public IQuery Query { get; private set; }
        public IScoreFunction[] Functions { get; private set; }

        public FunctionScoreQuery(IQuery query, IScoreFunction[] functions)
        {
            Query = query;
            Functions = functions;
        }

        public override QueryType Type => QueryType.FunctionScoreQuery;

        public override JsonObject ToJson()
        {
            var jo = new JsonObject();

            var query = Query == null ? null : Query.ToJson();
            if (query != null)
            {
                jo.Add("query", query);
            }

            jo.Add("functions", Functions.Select(e => e.ToJson()));

            return new JsonObject()
                .Add("function_score", jo);
        }
    }
}