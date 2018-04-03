
namespace YM.Elasticsearch.Query.TermQueries
{
    public class RangeValue
    {
        public object Value { get; private set; }
        public bool Include { get; private set; }

        public RangeValue(object value, bool include = true)
        {
            Value = value;
            Include = include;
        }
    }
}
