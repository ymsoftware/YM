using YM.Json;

namespace YM.Elasticsearch.Client.Search
{
    public class SearchResponse : JsonDocument
    {
        public int Took { get; private set; }
        public bool IsTimedOut { get; private set; }
        public SearchHits Hits { get; private set; }

        public SearchResponse(JsonObject jo)
        {
            Took = jo.Property<int>("took");
            IsTimedOut = jo.Property<bool>("timed_out");

            var hits = jo.Property<JsonObject>("hits");
            if (hits != null && !hits.IsEmpty)
            {
                Hits = new SearchHits(hits);
            }
        }

        public bool IsEmpty
        {
            get
            {
                return Hits.IsEmpty;
            }
        }

        public override JsonObject ToJson()
        {
            var jo = new JsonObject()
                .Add("took", Took)
                .Add("timed_out", IsTimedOut);

            if (Hits != null)
            {
                jo.Add("hits", Hits.ToJson());
            }

            return jo;
        }
    }
}
