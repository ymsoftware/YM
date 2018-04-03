using YM.Json;

namespace YM.Elasticsearch.Query
{
    public abstract class FieldQueryBase : QueryBase
    {
        public string Field { get; private set; }
        public object Value { get; private set; }

        public FieldQueryBase(string field, object value)
        {
            Field = field;
            Value = value;
        }

        public override JsonObject ToJson()
        {
            return new JsonObject().Add(Type.ToQueryName(), new JsonObject().Add(Field, Value));
        }
    }
}
