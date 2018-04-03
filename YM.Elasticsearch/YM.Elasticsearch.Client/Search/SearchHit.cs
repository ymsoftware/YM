using System.Linq;
using YM.Json;

namespace YM.Elasticsearch.Client.Search
{
    public class SearchHit : ElasticsearchDocument
    {
        public double Score { get; private set; }
        public object[] Sort { get; private set; }

        public SearchHit(JsonObject hit) : base(hit)
        {
            Score = hit.Property<double>("_score");

            var sort = hit.Property<JsonArray>("sort");
            if (sort != null && sort.Length > 0)
            {
                Sort = sort.Select(e => (object)e).ToArray();
            }
        }

        public override JsonObject ToJson()
        {
            var jo = new JsonObject()
                .Add("_id", Id)
                .Add("_index", Index)
                .Add("_type", Type);

            if (Version > 0)
            {
                jo.Add("_version", Version);
            }

            if (Score > 0)
            {
                jo.Add("_score", Score);
            }

            jo.Add("_source", Source);

            if (Sort != null)
            {
                jo.Add("sort", new JsonArray(Sort));
            }

            return jo;
        }
    }
}