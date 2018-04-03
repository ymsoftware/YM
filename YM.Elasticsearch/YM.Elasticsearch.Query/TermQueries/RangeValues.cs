namespace YM.Elasticsearch.Query.TermQueries
{
    public class RangeValues
    {
        public RangeValue From { get; private set; }
        public RangeValue To { get; private set; }

        public RangeValues(RangeValue from, RangeValue to)
        {
            From = from;
            To = to;
        }

        public RangeValues(object from, object to)
        {
            if (from != null)
            {
                From = new RangeValue(from);
            }

            if (to != null)
            {
                To = new RangeValue(to);
            }
        }
    }
}
