using YM.Json;

namespace YM.Elasticsearch.Client.Search
{
    public class ScrollRequest : RestRequest
    {
        public SearchRequest Search { get; private set; }
        public string KeepAlive { get; private set; }
        public string ScrollId { get; private set; }

        public ScrollRequest(SearchRequest request, string keepAlive = "1m")
        {
            Search = request;
            KeepAlive = keepAlive;
        }

        public ScrollRequest(string scrollId, string keepAlive = "1m")
        {
            ScrollId = scrollId;
            KeepAlive = keepAlive;
        }

        public override string GetBody()
        {
            return ToJson().ToString();
        }

        public override string GetUrl(string clusterUrl)
        {
            if (string.IsNullOrWhiteSpace(ScrollId))
            {
                return string.Format("{0}?scroll={1}", Search.GetUrl(clusterUrl), KeepAlive);
            }
            else
            {
                return clusterUrl + "/_search/scroll";
            }
        }

        public override JsonObject ToJson()
        {
            if (string.IsNullOrWhiteSpace(ScrollId))
            {
                return Search.ToJson();
            }
            else
            {
                return new JsonObject()
                    .Add("scroll", KeepAlive)
                    .Add("scroll_id", ScrollId);
            }
        }
    }
}
