using YM.Json;

namespace YM.Elasticsearch.Query
{
    public class MatchAllQuery : QueryBase
    {
        public override QueryType Type => QueryType.MatchAllQuery;

        public override JsonObject ToJson()
        {
            return new JsonObject().Add("match_all", new JsonObject());
        }
    }
}
