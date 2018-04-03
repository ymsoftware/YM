using YM.Json;

namespace YM.Elasticsearch.Client.Documents
{
    public class GetDocumentRequest : RestRequest
    {
        public string Id { get; private set; }
        public string Index { get; private set; }
        public string Type { get; private set; }

        public GetDocumentRequest(string id, string index, string type = "doc")
        {
            Id = id;
            Index = index;
            Type = type;
        }

        public override JsonObject ToJson()
        {
            return new JsonObject()
                .Add("_id", Id)
                .Add("_index", Index)
                .Add("_type", Type);
        }

        public override string GetUrl(string clusterUrl)
        {
            return string.Format("{0}/{1}/{2}/{3}", clusterUrl, Index, Type, Id);
        }

        public override string GetBody()
        {
            return null;
        }
    }
}
