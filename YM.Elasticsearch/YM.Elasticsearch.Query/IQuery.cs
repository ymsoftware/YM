using YM.Json;

namespace YM.Elasticsearch.Query
{
    public interface IQuery
    {
        QueryType Type { get; }
        JsonObject ToJson();
    }
}
