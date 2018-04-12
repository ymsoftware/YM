using YM.Json;

namespace YM.Elasticsearch.Query.SpanQueries
{
    public class SpanTermQuery : FieldQueryBase
    {
        public SpanTermQuery(string field, object value) : base(field, value) { }

        public override QueryType Type => QueryType.SpanTermQuery;

        public override JsonObject ToJson()
        {
            return new JsonObject()
                .Add("span_term", new JsonObject()
                    .Add(Field, Value));
        }
    }
}
