using Microsoft.Extensions.Configuration;
using YM.Elasticsearch;
using YM.Json;

namespace AP.Search.Web.Nancy
{
    class AppSettings : JsonDocument
    {
        public string ClusterUrl { get; private set; }
        public string Version { get; private set; }

        public static AppSettings Current { get; private set; }

        public AppSettings(IConfigurationRoot config)
        {
            foreach (var kvp in config.AsEnumerable())
            {
                switch (kvp.Key)
                {
                    case "cluster": ClusterUrl = kvp.Value; break;
                    case "version": Version = kvp.Value; break;
                }
            }

            Current = this;
        }

        public static void Init(IConfigurationRoot config)
        {
            new AppSettings(config);
        }

        public override JsonObject ToJson()
        {
            var jo = new JsonObject()
                .Add("version", Version)
                .Add("cluster", ClusterUrl);

            return jo;
        }
    }
}
