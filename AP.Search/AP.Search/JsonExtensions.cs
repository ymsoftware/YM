using YM.Json;

namespace AP.Search
{
    static class JsonExtensions
    {
        public static T Property<T>(this JsonObject jo, string[] names)
        {
            var jp = jo.Property(names);
            if (jp != null)
            {
                return jp.Value.Get<T>();
            }

            return default(T);
        }

        public static JsonProperty Property(this JsonObject jo, string[] names)
        {
            foreach (string name in names)
            {
                var jp = jo.Property(name);
                if (jp != null)
                {
                    return jp;
                }
            }

            return null;
        }        
    }
}