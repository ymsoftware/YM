using YM.Json;

namespace AP.Search.QueryExpansion
{
    public static class QueryExpanderRepository
    {
        public static IQueryExpander GetQueryExpander(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            switch (key.ToLower())
            {
                case "sg":
                case "simple_group":
                case "simple_grouping":
                case "simple-group":
                case "simple-grouping":
                case "simplegrouping":
                    return new SimpleGroupingQueryExpander();
            }

            return null;
        }

        public static IQueryExpander GetQueryExpander(JsonObject jo)
        {
            if (jo == null || jo.IsEmpty)
            {
                return null;
            }

            var jp = jo.Properties()[0];

            switch (jp.Name.ToLower())
            {
                case "px":
                case "proximity":
                    return GetProximityQueryExpander(jp.Value.Get<JsonObject>());
            }

            return null;
        }

        private static IQueryExpander GetProximityQueryExpander(JsonObject jo)
        {
            if (jo == null || jo.IsEmpty)
            {
                return null;
            }

            string field = jo.Property<string>("field");
            if (string.IsNullOrWhiteSpace(field))
            {
                return null;
            }

            return new ProximityQueryExpander(field, jo.Property<int>("slop"));
        }
    }
}
