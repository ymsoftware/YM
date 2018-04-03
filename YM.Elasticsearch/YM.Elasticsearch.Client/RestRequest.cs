namespace YM.Elasticsearch.Client
{
    public abstract class RestRequest : JsonDocument
    {
        public abstract string GetUrl(string clusterUrl);
        public abstract string GetBody();
    }
}
