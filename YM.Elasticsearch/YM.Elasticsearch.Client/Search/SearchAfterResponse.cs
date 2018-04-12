using System.Linq;

namespace YM.Elasticsearch.Client.Search
{
    public class SearchAfterResponse : SearchResponse
    {
        public object[] SearchAfter { get; private set; }

        public SearchAfterResponse(RestResponse response) : base(response)
        {
            if (!IsEmpty)
            {
                SearchAfter = Hits.Hits.Last().Sort;
            }
        }
    }
}
