using YM.Json;

namespace YM.Elasticsearch.Query
{
    public abstract class QueryBase : JsonDocument, IQuery
    {
        public abstract QueryType Type { get; }
    }
}
