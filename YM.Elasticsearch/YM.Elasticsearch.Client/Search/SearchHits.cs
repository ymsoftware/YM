using System.Linq;
using YM.Json;

namespace YM.Elasticsearch.Client.Search
{
    public class SearchHits : JsonDocument
    {
        public int Total { get; private set; }
        public double MaxScore { get; private set; }
        public SearchHit[] Hits { get; private set; }

        public SearchHits(JsonObject hits)
        {
            if (hits == null)
            {
                return;
            }

            Total = hits.Property<int>("total");
            MaxScore = hits.Property<double>("max_score");

            var ja = hits.Property<JsonArray>("hits");
            if (ja != null)
            {
                Hits = ja.Select(e => new SearchHit(e.Get<JsonObject>())).ToArray();
            }
        }

        public bool IsEmpty
        {
            get
            {
                return Hits == null || Hits.Length == 0;
            }
        }

        public override JsonObject ToJson()
        {
            var jo = new JsonObject()
                .Add("total", Total);

            if (MaxScore > 0)
            {
                jo.Add("max_score", MaxScore);
            }

            if (Hits != null)
            {
                var hits = Hits.Select(e => e.ToJson());
                jo.Add("hits", new JsonArray(hits));
            }

            return jo;
        }
    }
}