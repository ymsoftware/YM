using System.Linq;
using YM.Json;

namespace YM.Elasticsearch.Client.Search
{
    public class SearchAfterResponse : SearchResponse
    {
        public object[] SearchAfter { get; private set; }

        public SearchAfterResponse(JsonObject jo) : base(jo)
        {
            if (!IsEmpty)
            {
                SearchAfter = Hits.Hits.Last().Sort;
            }
        }
    }
}
