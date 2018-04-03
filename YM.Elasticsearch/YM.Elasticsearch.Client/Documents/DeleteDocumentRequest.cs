using System.Collections.Generic;
using YM.Json;

namespace YM.Elasticsearch.Client.Documents
{
    public class DeleteDocumentRequest : RestRequest
    {
        public string Id { get; private set; }
        public string Index { get; private set; }
        public string Type { get; private set; }
        public bool IsRefresh { get; private set; }
        public int Version { get; private set; }

        public DeleteDocumentRequest(string id, string index, string type = "doc", bool refresh = false, int version = 0)
        {
            Id = id;
            Index = index;
            Type = type;
            IsRefresh = refresh;
            Version = version;
        }

        public override JsonObject ToJson()
        {
            var jo = new JsonObject()
                .Add("_id", Id)
                .Add("_index", Index)
                .Add("_type", Type)
                .Add("refresh", IsRefresh);

            if (Version > 0)
            {
                jo.Add("_version", Version);
            };

            return jo;
        }

        public override string GetUrl(string clusterUrl)
        {
            var tokens = new List<string>();

            if (IsRefresh)
            {
                tokens.Add("refresh=true");
            }

            if (Version > 0)
            {
                tokens.Add("version=" + Version.ToString());
            }

            string query = tokens.Count == 0 ? "" : "?" + string.Join("&", tokens);

            return string.Format("{0}/{1}/{2}/{3}{4}", clusterUrl, Index, Type, Id, query);
        }

        public override string GetBody()
        {
            return null;
        }
    }
}
