using YM.Json;

namespace YM.Elasticsearch.Query.TermQueries
{
    public class TermsQuery : FieldQueryBase
    {
        public object[] Values { get; private set; }

        public TermsQuery(string field, object[] values) : base(field, values)
        {
            Values = values;
        }

        public override QueryType Type => QueryType.TermsQuery;

        public override JsonObject ToJson()
        {
            if (Values != null && Values.Length == 1)
            {
                return new JsonObject().Add("term", new JsonObject().Add(Field, Values[0]));
            }
            return base.ToJson();
        }
    }
}