using System.Collections.Generic;
using System.Linq;
using YM.Json;

namespace YM.Elasticsearch.Client.Documents
{
    public class BulkRequest : RestRequest
    {
        private readonly List<BulkRequestItem> _items = new List<BulkRequestItem>();

        public bool IsRefresh { get; private set; }

        public BulkRequest(bool refresh = false)
        {
            IsRefresh = refresh;
        }

        public BulkRequest Add(BulkRequestItem item)
        {
            _items.Add(item);
            return this;
        }

        public BulkRequest Add(IEnumerable<BulkRequestItem> items)
        {
            _items.AddRange(items);
            return this;
        }

        public BulkRequest Index(IEnumerable<ElasticsearchDocument> documents)
        {
            var items = documents.Select(e => new BulkRequestItem("index", e.Id, e.Index, e.Type, e.Source));
            return Add(items);
        }

        public override string GetBody()
        {
            return string.Join("\n", _items.Select(e => e.ToBulkString())) + "\n";
        }

        public override string GetUrl(string clusterUrl)
        {
            string query = IsRefresh ? "?refresh=true" : "";
            return string.Format("{0}/_bulk{1}", clusterUrl, query);
        }        

        public override JsonObject ToJson()
        {
            var items = new JsonArray();

            foreach(var item in _items)
            {
                items.Add(item.ToJson());
            }

            return new JsonObject()
                .Add("actions", items);
        }
    }
}
