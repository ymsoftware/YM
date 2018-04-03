using YM.Json;

namespace YM.Elasticsearch.Query.FullTextQueries
{
    public class MatchQuery : FieldQueryBase
    {
        public bool IsAnd { get; private set; }
        public bool IsZeroTerms { get; private set; }

        public MatchQuery(string field, string value, bool isAnd = false, bool isZeroTerms = false) : base(field, value)
        {
            IsAnd = isAnd;
            IsZeroTerms = isZeroTerms;
        }

        public override QueryType Type => QueryType.MatchQuery;

        public override JsonObject ToJson()
        {
            string value = (string)Value;
            bool isAnd = IsAnd && value.Has(' ');

            if (isAnd || IsZeroTerms)
            {
                var query = new JsonObject().Add("query", Value);

                if (isAnd)
                {
                    query.Add("operator", "and");
                }

                if (IsZeroTerms)
                {
                    query.Add("zero_terms_query", "all");
                }

                return new JsonObject().Add("match", new JsonObject().Add(Field, query));
            }

            return base.ToJson();
        }
    }
}
