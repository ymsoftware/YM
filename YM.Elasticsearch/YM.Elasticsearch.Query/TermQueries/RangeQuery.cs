using YM.Json;

namespace YM.Elasticsearch.Query.TermQueries
{
    public class RangeQuery : FieldQueryBase
    {
        public RangeValues Values { get; private set; }

        public RangeQuery(string field, RangeValues values) : base(field, values)
        {
            Values = values;
        }

        public RangeQuery(string field, object from, object to) : base(field, new RangeValues(from, to))
        {
            Values = new RangeValues(from, to);
        }

        public override QueryType Type => QueryType.RangeQuery;

        public override JsonObject ToJson()
        {
            var range = new JsonObject();

            if (Values != null)
            {
                if (Values.From != null)
                {
                    range.Add(Values.From.Include ? "gte" : "gt", Values.From.Value);
                }

                if (Values.To != null)
                {
                    range.Add(Values.To.Include ? "lte" : "lt", Values.To.Value);
                }
            }

            return new JsonObject().Add(Type.ToQueryName(), new JsonObject().Add(Field, range));
        }
    }
}
