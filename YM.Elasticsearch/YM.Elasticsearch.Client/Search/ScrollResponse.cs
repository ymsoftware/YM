namespace YM.Elasticsearch.Client.Search
{
    public class ScrollResponse : SearchResponse
    {
        public string ScrollId { get; private set; }

        public ScrollResponse(RestResponse response) : base(response)
        {
            if (IsSuccess)
            {
                ScrollId = ToJson().Property<string>("_scroll_id");
            }
        }
    }
}
