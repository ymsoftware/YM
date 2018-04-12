using YM.Json;

namespace YM.Elasticsearch.Query.JoiningQueries
{
    public class NestedQuery : QueryBase
    {
        public string Path { get; private set; }
        public IQuery Query { get; private set; }

        public NestedQuery(string path, IQuery query)
        {
            Path = path;
            Query = query;
        }

        public override QueryType Type => QueryType.NestedQuery;

        public override JsonObject ToJson()
        {
            return new JsonObject()
                .Add("nested", new JsonObject()
                    .Add("path", Path)
                    .Add("query", Query.ToJson()));
        }
    }
}
