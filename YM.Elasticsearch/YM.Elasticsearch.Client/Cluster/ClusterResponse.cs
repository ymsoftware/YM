using YM.Json;

namespace YM.Elasticsearch.Client.Cluster
{
    public class ClusterResponse : JsonDocument
    {
        private readonly JsonObject _response;

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string UUID { get; private set; }
        public string Version { get; private set; }
        public string LuceneVersion { get; private set; }

        public ClusterResponse(JsonObject response)
        {
            _response = response;

            Id = response.Property<string>("name");
            Name = response.Property<string>("cluster_name");
            UUID = response.Property<string>("cluster_uuid");

            var version = response.Property<JsonObject>("version");
            Version = version.Property<string>("number");
            LuceneVersion = version.Property<string>("lucene_version");
        }

        public float VersionNumber
        {
            get
            {
                return GetNumber(Version);
            }
        }

        public float LuceneVersionNumber
        {
            get
            {
                return GetNumber(LuceneVersion);
            }
        }

        public override JsonObject ToJson()
        {
            return _response;
        }

        private float GetNumber(string number)
        {
            var tokens = number.Split('.');
            if (tokens.Length == 1)
            {
                return float.Parse(tokens[0]);
            }
            else
            {
                return float.Parse(string.Format("{0}.{1}", tokens[0], tokens[1]));
            }
        }
    }
}