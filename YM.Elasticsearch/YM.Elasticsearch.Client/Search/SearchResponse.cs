using System.Net;
using YM.Json;

namespace YM.Elasticsearch.Client.Search
{
    public class SearchResponse : JsonDocument
    {
        private readonly string _json;

        private JsonObject _jo = null;
        private int? _took = null;
        private bool? _timeout = null;
        private SearchHits _hits = null;

        public bool IsSuccess { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }

        public int Took
        {
            get
            {
                if (_took == null)
                {
                    _took = ToJson().Property<int>("took");
                }

                return _took.Value;
            }
        }

        public bool IsTimedOut
        {
            get
            {
                if (_timeout == null)
                {
                    _timeout = ToJson().Property<bool>("timed_out");
                }

                return _timeout.Value;
            }
        }

        public SearchHits Hits
        {
            get
            {
                if (_hits == null)
                {
                    _hits = new SearchHits(ToJson().Property<JsonObject>("hits"));
                }

                return _hits;
            }
        }

        public SearchResponse(RestResponse response)
        {
            _json = response.Content;
            StatusCode = response.Http.StatusCode;
            IsSuccess = response.Http.IsSuccessStatusCode;
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
            if (_jo == null)
            {
                _jo = JsonObject.Parse(_json);
            }

            return _jo;
        }

        public override string ToString()
        {
            return _json;
        }
    }
}
