using System.Linq;

namespace YM.Elasticsearch.Query.TermQueries
{
    public class TermsQuery<T> : TermsQuery
    {
        public new T[] Values { get; private set; }

        public TermsQuery(string field, T[] values) : base(field, values.Select(e => (object)e).ToArray())
        {
            Values = values;
        }
    }
}