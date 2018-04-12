using YM.Json;

namespace YM.Elasticsearch.Client.Search
{
    public class SearchResponse : JsonDocument
    {
        private readonly JsonObject _jo;

        public bool IsSuccess { get; private set; }
        public int Took { get; private set; }
        public bool IsTimedOut { get; private set; }
        public SearchHits Hits { get; private set; }

        public SearchResponse(RestResponse response)
        {
            _jo = JsonObject.Parse(response.Content);

            if (response.Http.IsSuccessStatusCode)
            {
                IsSuccess = true;                

                Took = _jo.Property<int>("took");
                IsTimedOut = _jo.Property<bool>("timed_out");

                var hits = _jo.Property<JsonObject>("hits");
                if (hits != null && !hits.IsEmpty)
                {
                    Hits = new SearchHits(hits);
                }
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
            return _jo;
        }
    }
}
