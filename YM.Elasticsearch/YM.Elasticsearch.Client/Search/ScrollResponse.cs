using YM.Json;

namespace YM.Elasticsearch.Client.Search
{
    public class ScrollResponse : SearchResponse
    {
        public string ScrollId { get; private set; }

        public ScrollResponse(JsonObject jo) : base(jo)
        {
            ScrollId = jo.Property<string>("_scroll_id");
        }

        public override JsonObject ToJson()
        {
            return base.ToJson().Add("_scroll_id", ScrollId);
        }
    }
}
