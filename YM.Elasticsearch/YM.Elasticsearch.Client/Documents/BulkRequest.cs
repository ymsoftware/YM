using System.Collections.Generic;
using System.Linq;
using System.Text;
using YM.Json;

namespace YM.Elasticsearch.Client.Documents
{
    public class BulkRequest : RestRequest
    {
        private readonly List<BulkRequestItem> _items = new List<BulkRequestItem>();

        public bool IsRefresh { get; private set; }
        public string Pipeline { get; private set; }

        public BulkRequest(bool refresh = false, string pipeline = null)
        {
            IsRefresh = refresh;
            Pipeline = pipeline;
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
            var sb = new StringBuilder();

            if (_items.Count > 0)
            {
                sb.Append(string.Join("\n", _items.Select(e => e.ToBulkString())));
                sb.Append("\n");
            }            

            return sb.ToString();
        }

        public override string GetUrl(string clusterUrl)
        {
            var tokens = new List<string>();

            if (IsRefresh)
            {
                tokens.Add("refresh=true");
            }

            if (!string.IsNullOrWhiteSpace(Pipeline))
            {
                tokens.Add("pipeline=" + Pipeline);
            }

            string query = tokens.Count == 0 ? "" : "?" + string.Join("&", tokens);

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
