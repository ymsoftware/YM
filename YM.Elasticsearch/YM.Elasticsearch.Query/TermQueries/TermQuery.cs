using YM.Json;

namespace YM.Elasticsearch.Query.TermQueries
{
    public class TermQuery : FieldQueryBase
    {
        public TermQuery(string field, object value) : base(field, value) { }

        public override QueryType Type => QueryType.TermQuery;

        public override JsonObject ToJson()
        {
            if (Value is JsonArray)
            {
                return new JsonObject().Add("terms", new JsonObject().Add(Field, Value));
            }

            return base.ToJson();
        }
    }
}
