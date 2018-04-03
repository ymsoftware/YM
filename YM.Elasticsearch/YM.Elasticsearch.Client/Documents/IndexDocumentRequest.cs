using System.Collections.Generic;
using YM.Json;

namespace YM.Elasticsearch.Client.Documents
{
    public class IndexDocumentRequest : RestRequest
    {
        public ElasticsearchDocument Document { get; private set; }
        public bool IsRefresh { get; private set; }
        public string Pipeline { get; private set; }
        public int Version { get; private set; }

        public IndexDocumentRequest(ElasticsearchDocument document, bool refresh = false, string pipeline = null, int version = 0)
        {
            Document = document;
            IsRefresh = refresh;
            Pipeline = pipeline;
            Version = version;
        }

        public override JsonObject ToJson()
        {
            var jo = new JsonObject()
                .Add("document", Document.ToJson())
                .Add("refresh", IsRefresh);

            if (!string.IsNullOrWhiteSpace(Pipeline))
            {
                jo.Add("pipeline", Pipeline);
            }

            return jo;
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

            if (Version > 0)
            {
                tokens.Add("version=" + Version.ToString());
            }

            string query = tokens.Count == 0 ? "" : "?" + string.Join("&", tokens);

            return string.Format("{0}/{1}/{2}/{3}{4}", clusterUrl, Document.Index, Document.Type, Document.Id, query);
        }

        public override string GetBody()
        {
            return Document.Source.ToString();
        }
    }
}
