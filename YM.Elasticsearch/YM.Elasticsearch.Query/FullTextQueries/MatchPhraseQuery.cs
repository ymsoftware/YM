using YM.Json;

namespace YM.Elasticsearch.Query.FullTextQueries
{
    public class MatchPhraseQuery : FieldQueryBase
    {
        public int Slop { get; private set; }

        public MatchPhraseQuery(string field, string value, int slop = 0) : base(field, value)
        {
            Slop = slop;
        }

        public override QueryType Type => QueryType.MatchPhraseQuery;

        public override JsonObject ToJson()
        {
            if (Slop > 0)
            {
                return new JsonObject()
                    .Add("match_phrase", new JsonObject()
                        .Add(Field, Value)
                        .Add("slop", Slop));
            }

            return base.ToJson();
        }
    }
}
