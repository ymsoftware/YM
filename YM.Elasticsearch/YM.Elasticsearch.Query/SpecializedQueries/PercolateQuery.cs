using YM.Json;

namespace YM.Elasticsearch.Query.SpecializedQueries
{
    public class PercolateQuery : QueryBase
    {
        public string Field { get; private set; }
        public JsonObject Document { get; private set; }

        public PercolateQuery(string field, JsonObject document)
        {
            Field = field;
            Document = document;
        }

        public override QueryType Type => QueryType.PercolateQuery;

        public override JsonObject ToJson()
        {
            return new JsonObject()
                .Add("percolate", new JsonObject()
                    .Add("field", Field)
                    .Add("document", Document));
        }
    }
}
